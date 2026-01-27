using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.DTOs;
using TaskHub.API.Models;

namespace TaskHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly TaskHubDbContext _context;

        public NewsController(TaskHubDbContext context)
        {
            _context = context;
        }

        // GET: api/news
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NewsDto>>> GetNews()
        {
            var news = await _context.News
                .Include(n => n.Author)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NewsDto
                {
                    NewsID = n.NewsID,
                    Title = n.Title,
                    Content = n.Content,
                    ImageUrl = n.ImageUrl,
                    AuthorName = n.Author.Name,
                    CreatedAt = n.CreatedAt.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            return Ok(news);
        }

        // GET: api/news/5
        [HttpGet("{id}")]
        public async Task<ActionResult<NewsDto>> GetNewsItem(int id)
        {
            var news = await _context.News
                .Include(n => n.Author)
                .Where(n => n.NewsID == id)
                .Select(n => new NewsDto
                {
                    NewsID = n.NewsID,
                    Title = n.Title,
                    Content = n.Content,
                    ImageUrl = n.ImageUrl,
                    AuthorName = n.Author.Name,
                    CreatedAt = n.CreatedAt.ToString("dd/MM/yyyy")
                })
                .FirstOrDefaultAsync();

            if (news == null)
            {
                return NotFound();
            }

            return Ok(news);
        }

        // POST: api/news
        [HttpPost]
        public async Task<ActionResult<NewsDto>> CreateNews(CreateNewsDto createDto)
        {
            var news = new News
            {
                Title = createDto.Title,
                Content = createDto.Content,
                ImageUrl = createDto.ImageUrl,
                AuthorID = createDto.AuthorID,
                CreatedAt = DateTime.Now
            };

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            // Load the full news with author
            var createdNews = await _context.News
                .Include(n => n.Author)
                .Where(n => n.NewsID == news.NewsID)
                .Select(n => new NewsDto
                {
                    NewsID = n.NewsID,
                    Title = n.Title,
                    Content = n.Content,
                    ImageUrl = n.ImageUrl,
                    AuthorName = n.Author.Name,
                    CreatedAt = n.CreatedAt.ToString("dd/MM/yyyy")
                })
                .FirstOrDefaultAsync();

            return CreatedAtAction(nameof(GetNewsItem), new { id = news.NewsID }, createdNews);
        }

        // PUT: api/news/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNews(int id, UpdateNewsDto updateDto)
        {
            var news = await _context.News.FindAsync(id);
            
            if (news == null)
            {
                return NotFound();
            }

            news.Title = updateDto.Title;
            news.Content = updateDto.Content;
            news.ImageUrl = updateDto.ImageUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NewsExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/news/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNews(int id)
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.NewsID == id);
        }
    }
}
