using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.DTOs;

namespace TaskHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TaskHubDbContext _context;

        public AuthController(TaskHubDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email && u.Password == loginDto.Password);

            if (user == null)
            {
                return Unauthorized(new { message = "არასწორი ელ-ფოსტა ან პაროლი" });
            }

            var response = new LoginResponseDto
            {
                UserId = user.UserID,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Title = user.Title,
                Initials = user.Initials,
                Department = user.Department
            };

            return Ok(response);
        }

        // GET: api/auth/demo-credentials (for demo purposes only)
        [HttpGet("demo-credentials")]
        public async Task<ActionResult<IEnumerable<UserWithPasswordDto>>> GetDemoCredentials()
        {
            var users = await _context.Users
                .Select(u => new UserWithPasswordDto
                {
                    UserID = u.UserID,
                    Name = u.Name,
                    Email = u.Email,
                    Password = u.Password,
                    Role = u.Role
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
