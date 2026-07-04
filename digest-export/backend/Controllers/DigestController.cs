using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourProject.API.Data;       // REPLACE with your actual namespace
using YourProject.API.DTOs;       // REPLACE with your actual namespace
using YourProject.API.Models;     // REPLACE with your actual namespace

namespace YourProject.API.Controllers   // REPLACE with your actual namespace
{
    [ApiController]
    [Route("api/[controller]")]
    public class DigestController : ControllerBase
    {
        private readonly YourDbContext _context;   // REPLACE "YourDbContext" with your actual DbContext class name

        public DigestController(YourDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DigestEntryDto>>> GetDigestEntries([FromQuery] bool activeOnly = false)
        {
            var query = _context.DigestEntries.Include(d => d.Author).AsQueryable();

            if (activeOnly)
            {
                var now = DateTime.Now;
                query = query.Where(d => d.IsActive && d.PeriodFrom <= now && d.PeriodTo >= now);
            }

            var entries = await query
                .OrderByDescending(d => d.CreatedAt)
                .Select(d => new DigestEntryDto
                {
                    DigestEntryID = d.DigestEntryID,
                    Title = d.Title,
                    Description = d.Description,
                    ImageUrl = d.ImageUrl,
                    SourceName = d.SourceName,
                    SourceUrl = d.SourceUrl,
                    PeriodFrom = d.PeriodFrom.ToString("yyyy-MM-dd"),
                    PeriodTo = d.PeriodTo.ToString("yyyy-MM-dd"),
                    IsFeatured = d.IsFeatured,
                    IsActive = d.IsActive,
                    AuthorName = d.Author.Name,
                    CreatedAt = d.CreatedAt.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Ok(entries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DigestEntryDto>> GetDigestEntry(int id)
        {
            var entry = await _context.DigestEntries
                .Include(d => d.Author)
                .Where(d => d.DigestEntryID == id)
                .Select(d => new DigestEntryDto
                {
                    DigestEntryID = d.DigestEntryID,
                    Title = d.Title,
                    Description = d.Description,
                    ImageUrl = d.ImageUrl,
                    SourceName = d.SourceName,
                    SourceUrl = d.SourceUrl,
                    PeriodFrom = d.PeriodFrom.ToString("yyyy-MM-dd"),
                    PeriodTo = d.PeriodTo.ToString("yyyy-MM-dd"),
                    IsFeatured = d.IsFeatured,
                    IsActive = d.IsActive,
                    AuthorName = d.Author.Name,
                    CreatedAt = d.CreatedAt.ToString("dd/MM/yyyy")
                })
                .FirstOrDefaultAsync();

            if (entry == null) return NotFound();
            return Ok(entry);
        }

        [HttpPost]
        public async Task<ActionResult<DigestEntryDto>> CreateDigestEntry(CreateDigestEntryDto createDto)
        {
            var entry = new DigestEntry
            {
                Title = createDto.Title,
                Description = createDto.Description,
                ImageUrl = createDto.ImageUrl,
                SourceName = createDto.SourceName,
                SourceUrl = createDto.SourceUrl,
                PeriodFrom = createDto.PeriodFrom,
                PeriodTo = createDto.PeriodTo,
                IsFeatured = createDto.IsFeatured,
                IsActive = createDto.IsActive,
                AuthorID = createDto.AuthorID,
                CreatedAt = DateTime.Now
            };

            _context.DigestEntries.Add(entry);
            await _context.SaveChangesAsync();

            var created = await _context.DigestEntries
                .Include(d => d.Author)
                .Where(d => d.DigestEntryID == entry.DigestEntryID)
                .Select(d => new DigestEntryDto
                {
                    DigestEntryID = d.DigestEntryID,
                    Title = d.Title,
                    Description = d.Description,
                    ImageUrl = d.ImageUrl,
                    SourceName = d.SourceName,
                    SourceUrl = d.SourceUrl,
                    PeriodFrom = d.PeriodFrom.ToString("yyyy-MM-dd"),
                    PeriodTo = d.PeriodTo.ToString("yyyy-MM-dd"),
                    IsFeatured = d.IsFeatured,
                    IsActive = d.IsActive,
                    AuthorName = d.Author.Name,
                    CreatedAt = d.CreatedAt.ToString("dd/MM/yyyy")
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetDigestEntry), new { id = entry.DigestEntryID }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDigestEntry(int id, UpdateDigestEntryDto updateDto)
        {
            var entry = await _context.DigestEntries.FindAsync(id);
            if (entry == null) return NotFound();

            entry.Title = updateDto.Title;
            entry.Description = updateDto.Description;
            entry.ImageUrl = updateDto.ImageUrl;
            entry.SourceName = updateDto.SourceName;
            entry.SourceUrl = updateDto.SourceUrl;
            entry.PeriodFrom = updateDto.PeriodFrom;
            entry.PeriodTo = updateDto.PeriodTo;
            entry.IsFeatured = updateDto.IsFeatured;
            entry.IsActive = updateDto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DigestEntries.Any(e => e.DigestEntryID == id)) return NotFound();
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDigestEntry(int id)
        {
            var entry = await _context.DigestEntries.FindAsync(id);
            if (entry == null) return NotFound();

            _context.DigestEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
