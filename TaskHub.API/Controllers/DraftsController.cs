using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TaskHub.API.Data;
using TaskHub.API.DTOs;
using TaskHub.API.Models;

namespace TaskHub.API.Controllers
{
    // Generic replacement for the old DigestDraftsController: persists a
    // "minimized" draft per (user, moduleType) so any admin module's in-progress
    // form can survive a minimize/page refresh, without needing its own table,
    // controller, or migration. See Draft.cs for the JSON-payload design
    // rationale.
    //
    // Auth follows the same convention as the rest of the API (AuthController,
    // ProfileController, the old DigestDraftsController, etc.): there is no
    // token/JWT/session layer, so the acting user's id is passed explicitly by
    // the client as a route parameter.
    [ApiController]
    [Route("api/[controller]")]
    public class DraftsController : ControllerBase
    {
        private readonly TaskHubDbContext _context;

        public DraftsController(TaskHubDbContext context)
        {
            _context = context;
        }

        // GET: api/drafts/{userId}
        // Returns ALL of the user's drafts across every module, so the frontend
        // can restore every minimized bubble on app load in one request.
        [HttpGet("{userId}")]
        public async Task<ActionResult<List<DraftDto>>> GetDrafts(int userId)
        {
            var drafts = await _context.Drafts
                .AsNoTracking()
                .Where(d => d.UserId == userId)
                .ToListAsync();

            return Ok(drafts.Select(ToDto).ToList());
        }

        // GET: api/drafts/{userId}/{moduleType}
        // Returns the user's draft for one module, or 204 No Content if none exists.
        [HttpGet("{userId}/{moduleType}")]
        public async Task<ActionResult<DraftDto>> GetDraft(int userId, string moduleType)
        {
            var draft = await _context.Drafts
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId && d.ModuleType == moduleType);

            if (draft == null) return NoContent();

            return Ok(ToDto(draft));
        }

        // PUT: api/drafts/{userId}/{moduleType}
        // Upsert: creates the user's draft for this module if absent, otherwise
        // overwrites the existing one. Combined with the unique index on
        // (UserId, ModuleType), a user can never accumulate more than one
        // minimized draft per module - while still being free to have, say, a
        // digest draft AND a vacancy draft minimized at the same time.
        [HttpPut("{userId}/{moduleType}")]
        public async Task<ActionResult<DraftDto>> SaveDraft(int userId, string moduleType, SaveDraftDto dto)
        {
            if (!DraftModuleTypes.All.Contains(moduleType))
                return BadRequest($"Unknown moduleType '{moduleType}'.");

            var draft = await _context.Drafts
                .FirstOrDefaultAsync(d => d.UserId == userId && d.ModuleType == moduleType);

            if (draft == null)
            {
                draft = new Draft { UserId = userId, ModuleType = moduleType };
                _context.Drafts.Add(draft);
            }

            draft.DraftDataJson = dto.DraftData.GetRawText();
            draft.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(ToDto(draft));
        }

        // DELETE: api/drafts/{userId}/{moduleType}
        // Clears the user's draft for one module (e.g. when its modal is
        // restored and submitted, or the user discards it). No-op-safe: returns
        // 204 even if nothing existed.
        [HttpDelete("{userId}/{moduleType}")]
        public async Task<IActionResult> DeleteDraft(int userId, string moduleType)
        {
            var draft = await _context.Drafts
                .FirstOrDefaultAsync(d => d.UserId == userId && d.ModuleType == moduleType);

            if (draft != null)
            {
                _context.Drafts.Remove(draft);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        private static DraftDto ToDto(Draft d) => new DraftDto
        {
            ModuleType = d.ModuleType,
            DraftData = JsonSerializer.Deserialize<JsonElement>(d.DraftDataJson),
            LastUpdated = d.LastUpdated.ToString("dd/MM/yyyy HH:mm")
        };
    }
}
