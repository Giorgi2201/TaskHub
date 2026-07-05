using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.DTOs;
using TaskHub.API.Models;

namespace TaskHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly TaskHubDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        // Demo/onboarding convenience only: the seeded demo accounts' ORIGINAL
        // plaintext passwords, hardcoded here since `Users.Password` now stores
        // a one-way hash and the real value can no longer be read back out of
        // the database. These match exactly what TaskHubDbContext.SeedData
        // originally inserted, so the "try demo account" panel on the login
        // page keeps working post-hashing. This is NOT a general mechanism for
        // recovering any user's password — it only ever covers these 4 fixed
        // demo emails.
        private static readonly Dictionary<string, string> DemoPasswordsByEmail = new()
        {
            ["g.maisuradze@railway.ge"] = "password1",
            ["n.beridze@railway.ge"] = "password2",
            ["d.kvaratskhelia@railway.ge"] = "password3",
            ["m.gelashvili@railway.ge"] = "admin123"
        };

        public AuthController(TaskHubDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            // Passwords are hashed, so we can no longer filter by password in the
            // query itself — look the user up by email only, then verify the hash.
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || !VerifyPassword(user, loginDto.Password))
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
            // EF Core can't translate a Dictionary.ContainsKey lookup into SQL, so
            // filter on the plain list of demo emails instead.
            var demoEmails = DemoPasswordsByEmail.Keys.ToList();
            var users = await _context.Users
                .Where(u => demoEmails.Contains(u.Email))
                .Select(u => new { u.UserID, u.Name, u.Email, u.Role })
                .ToListAsync();

            // Password comes from the hardcoded demo map, not the (now-hashed) DB column.
            var result = users.Select(u => new UserWithPasswordDto
            {
                UserID = u.UserID,
                Name = u.Name,
                Email = u.Email,
                Password = DemoPasswordsByEmail[u.Email],
                Role = u.Role
            });

            return Ok(result);
        }

        // Verifies a supplied plaintext password against the user's stored hash.
        // Guards against FormatException from VerifyHashedPassword in the unlikely
        // case a row still holds a non-hash value (e.g. the startup migration
        // hasn't run yet) so a bad row fails login cleanly instead of throwing.
        private bool VerifyPassword(User user, string suppliedPassword)
        {
            try
            {
                var result = _passwordHasher.VerifyHashedPassword(user, user.Password, suppliedPassword);
                return result == PasswordVerificationResult.Success
                    || result == PasswordVerificationResult.SuccessRehashNeeded;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
