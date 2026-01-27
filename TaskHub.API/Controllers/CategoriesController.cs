using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.DTOs;

namespace TaskHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly TaskHubDbContext _context;

        public CategoriesController(TaskHubDbContext context)
        {
            _context = context;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetCategories()
        {
            var categories = await _context.Categories
                .Include(c => c.Subcategories)
                .Select(c => new CategoryDto
                {
                    CategoryID = c.CategoryID,
                    CategoryName = c.CategoryName,
                    Subcategories = c.Subcategories.Select(s => new SubcategoryDto
                    {
                        SubcategoryID = s.SubcategoryID,
                        SubcategoryName = s.SubcategoryName
                    }).ToList()
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Subcategories)
                .FirstOrDefaultAsync(c => c.CategoryID == id);

            if (category == null)
            {
                return NotFound();
            }

            var categoryDto = new CategoryDto
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                Subcategories = category.Subcategories.Select(s => new SubcategoryDto
                {
                    SubcategoryID = s.SubcategoryID,
                    SubcategoryName = s.SubcategoryName
                }).ToList()
            };

            return Ok(categoryDto);
        }
    }
}
