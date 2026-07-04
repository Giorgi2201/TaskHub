using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.DTOs;
using TaskHub.API.Models;

namespace TaskHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VacanciesController : ControllerBase
    {
        private readonly TaskHubDbContext _context;

        public VacanciesController(TaskHubDbContext context)
        {
            _context = context;
        }

        // GET: api/vacancies
        // Optional query param: ?activeOnly=true — returns only active, non-expired vacancies (public page)
        // On every call, expired vacancies are written to inactive in the DB so the state is always current.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VacancyDto>>> GetVacancies([FromQuery] bool activeOnly = false)
        {
            // Expire any vacancies whose deadline has passed — persist the change immediately
            var expired = await _context.Vacancies
                .Where(v => v.IsActive && v.Deadline.HasValue && v.Deadline <= DateTime.Now)
                .ToListAsync();

            if (expired.Count > 0)
            {
                foreach (var v in expired)
                    v.IsActive = false;

                await _context.SaveChangesAsync();
            }

            var query = _context.Vacancies
                .Include(v => v.Author)
                .AsQueryable();

            if (activeOnly)
                query = query.Where(v => v.IsActive);

            var vacancies = await query
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VacancyDto
                {
                    VacancyID = v.VacancyID,
                    Title = v.Title,
                    Category = v.Category,
                    Department = v.Department,
                    Location = v.Location,
                    Description = v.Description,
                    Deadline = v.Deadline.HasValue ? v.Deadline.Value.ToString("yyyy-MM-ddTHH:mm") : null,
                    IsActive = v.IsActive,
                    AuthorName = v.Author.Name,
                    CreatedAt = v.CreatedAt.ToString("yyyy-MM-dd")
                })
                .ToListAsync();

            return Ok(vacancies);
        }

        // GET: api/vacancies/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VacancyDto>> GetVacancy(int id)
        {
            var vacancy = await _context.Vacancies
                .Include(v => v.Author)
                .Where(v => v.VacancyID == id)
                .Select(v => new VacancyDto
                {
                    VacancyID = v.VacancyID,
                    Title = v.Title,
                    Category = v.Category,
                    Department = v.Department,
                    Location = v.Location,
                    Description = v.Description,
                    Deadline = v.Deadline.HasValue ? v.Deadline.Value.ToString("yyyy-MM-ddTHH:mm") : null,
                    IsActive = v.IsActive,
                    AuthorName = v.Author.Name,
                    CreatedAt = v.CreatedAt.ToString("yyyy-MM-dd")
                })
                .FirstOrDefaultAsync();

            if (vacancy == null)
                return NotFound();

            return Ok(vacancy);
        }

        // POST: api/vacancies
        [HttpPost]
        public async Task<ActionResult<VacancyDto>> CreateVacancy(CreateVacancyDto createDto)
        {
            var vacancy = new Vacancy
            {
                Title = createDto.Title,
                Category = createDto.Category,
                Department = createDto.Department,
                Location = createDto.Location,
                Description = createDto.Description,
                Deadline = createDto.Deadline,
                IsActive = createDto.IsActive,
                AuthorID = createDto.AuthorID,
                CreatedAt = DateTime.Now
            };

            _context.Vacancies.Add(vacancy);
            await _context.SaveChangesAsync();

            var createdVacancy = await _context.Vacancies
                .Include(v => v.Author)
                .Where(v => v.VacancyID == vacancy.VacancyID)
                .Select(v => new VacancyDto
                {
                    VacancyID = v.VacancyID,
                    Title = v.Title,
                    Category = v.Category,
                    Department = v.Department,
                    Location = v.Location,
                    Description = v.Description,
                    Deadline = v.Deadline.HasValue ? v.Deadline.Value.ToString("yyyy-MM-ddTHH:mm") : null,
                    IsActive = v.IsActive,
                    AuthorName = v.Author.Name,
                    CreatedAt = v.CreatedAt.ToString("yyyy-MM-dd")
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetVacancy), new { id = vacancy.VacancyID }, createdVacancy);
        }

        // PUT: api/vacancies/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVacancy(int id, UpdateVacancyDto updateDto)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);

            if (vacancy == null)
                return NotFound();

            vacancy.Title = updateDto.Title;
            vacancy.Category = updateDto.Category;
            vacancy.Department = updateDto.Department;
            vacancy.Location = updateDto.Location;
            vacancy.Description = updateDto.Description;
            vacancy.Deadline = updateDto.Deadline;
            vacancy.IsActive = updateDto.IsActive;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VacancyExists(id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // DELETE: api/vacancies/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVacancy(int id)
        {
            var vacancy = await _context.Vacancies.FindAsync(id);
            if (vacancy == null)
                return NotFound();

            _context.Vacancies.Remove(vacancy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VacancyExists(int id)
        {
            return _context.Vacancies.Any(v => v.VacancyID == id);
        }
    }
}
