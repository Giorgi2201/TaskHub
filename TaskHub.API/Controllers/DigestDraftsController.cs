using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.DTOs;
using TaskHub.API.Models;

namespace TaskHub.API.Controllers
{
    // Persists the "minimized" digest draft so it survives a page refresh.
    // Completely separate from DigestController / DigestEntries: this only ever
    // touches the standalone DigestDrafts table, one row per user.
    //
    // Auth follows the same convention as the rest of the API (AuthController,
    // RequestsController, etc.): there is no token/JWT layer, so the acting
    // user's id is passed explicitly by the client (route param or DTO field).
    [ApiController]
    [Route("api/[controller]")]
    public class DigestDraftsController : ControllerBase
    {
        private readonly TaskHubDbContext _context;

        public DigestDraftsController(TaskHubDbContext context)
        {
            _context = context;
        }

        // GET: api/digestdrafts/{userId}
        // Returns the user's saved draft, or 204 No Content if none exists.
        [HttpGet("{userId}")]
        public async Task<ActionResult<DigestDraftDto>> GetDraft(int userId)
        {
            var draft = await _context.DigestDrafts
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserID == userId);

            if (draft == null) return NoContent();

            return Ok(ToDto(draft));
        }

        // POST: api/digestdrafts
        // Upsert: creates the user's draft if absent, otherwise overwrites the
        // existing one. Combined with the unique index on UserID this guarantees
        // a user can never accumulate more than one minimized draft.
        [HttpPost]
        public async Task<ActionResult<DigestDraftDto>> SaveDraft(SaveDigestDraftDto dto)
        {
            var draft = await _context.DigestDrafts
                .FirstOrDefaultAsync(d => d.UserID == dto.UserID);

            if (draft == null)
            {
                draft = new DigestDraft { UserID = dto.UserID };
                _context.DigestDrafts.Add(draft);
            }

            draft.Title = dto.Title;
            draft.Description = dto.Description;
            draft.ImageUrl = dto.ImageUrl;
            draft.SourceName = dto.SourceName;
            draft.SourceUrl = dto.SourceUrl;
            draft.PeriodFrom = dto.PeriodFrom;
            draft.PeriodTo = dto.PeriodTo;
            draft.IsFeatured = dto.IsFeatured;
            draft.IsActive = dto.IsActive;
            draft.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            return Ok(ToDto(draft));
        }

        // DELETE: api/digestdrafts/{userId}
        // Clears the user's draft (e.g. when the modal is restored and submitted,
        // or the user discards it). No-op-safe: returns 204 even if nothing existed.
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteDraft(int userId)
        {
            var draft = await _context.DigestDrafts
                .FirstOrDefaultAsync(d => d.UserID == userId);

            if (draft != null)
            {
                _context.DigestDrafts.Remove(draft);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        private static DigestDraftDto ToDto(DigestDraft d) => new DigestDraftDto
        {
            UserID = d.UserID,
            Title = d.Title,
            Description = d.Description,
            ImageUrl = d.ImageUrl,
            SourceName = d.SourceName,
            SourceUrl = d.SourceUrl,
            PeriodFrom = d.PeriodFrom?.ToString("yyyy-MM-dd"),
            PeriodTo = d.PeriodTo?.ToString("yyyy-MM-dd"),
            IsFeatured = d.IsFeatured,
            IsActive = d.IsActive,
            LastUpdated = d.LastUpdated.ToString("dd/MM/yyyy HH:mm")
        };
    }
}
