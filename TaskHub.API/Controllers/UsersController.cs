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
    public class UsersController : ControllerBase
    {
        private readonly TaskHubDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UsersController(TaskHubDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserManagementDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserManagementDto
                {
                    UserID = u.UserID,
                    Name = u.Name,
                    Role = u.Role,
                    Department = u.Department,
                    Email = u.Email,
                    Title = u.Title
                })
                .ToListAsync();

            return Ok(users);
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserManagementDto>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userDto = new UserManagementDto
            {
                UserID = user.UserID,
                Name = user.Name,
                Role = user.Role,
                Department = user.Department,
                Email = user.Email,
                Title = user.Title
            };

            return Ok(userDto);
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<UserManagementDto>> CreateUser(CreateUserDto createDto)
        {
            // Generate initials from name
            var nameParts = createDto.Name.Split(' ');
            var initials = nameParts.Length >= 2 
                ? $"{nameParts[0][0]}{nameParts[1][0]}" 
                : nameParts[0].Substring(0, Math.Min(2, nameParts[0].Length));

            // Determine avatar class based on role
            var avatarClass = createDto.Role switch
            {
                "შემვსები" => "avatar-blue",
                "ხელმძღვანელი" => "avatar-green", // Green
                "შემსრულებელი" => "avatar-orange", // Orange
                "ადმინისტრატორი" => "avatar-red",
                _ => "avatar-blue"
            };

            // Generate random 8-character password
            var password = GenerateRandomPassword(8);

            // Generate random phone number in format +995 5** ** ** **
            var phone = GenerateRandomPhone();

            var user = new User
            {
                Name = createDto.Name,
                Email = createDto.Email,
                Phone = phone,
                Role = createDto.Role,
                Department = createDto.Department,
                Title = createDto.Title,
                Initials = initials.ToUpper(),
                AvatarClass = avatarClass
            };
            // Hash before storing — never persist the generated plaintext password.
            user.Password = _passwordHasher.HashPassword(user, password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = new UserManagementDto
            {
                UserID = user.UserID,
                Name = user.Name,
                Role = user.Role,
                Department = user.Department,
                Email = user.Email,
                Title = user.Title
            };

            return CreatedAtAction(nameof(GetUser), new { id = user.UserID }, result);
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateRandomPhone()
        {
            var random = new Random();
            // Generate phone in format +995 5XX XX XX XX
            var firstPart = random.Next(50, 100); // 50-99
            var secondPart = random.Next(10, 100); // 10-99
            var thirdPart = random.Next(10, 100); // 10-99
            var fourthPart = random.Next(10, 100); // 10-99
            
            return $"+995 {firstPart} {secondPart:D2} {thirdPart:D2} {fourthPart:D2}";
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FindAsync(id);
            
            if (user == null)
            {
                return NotFound();
            }

            user.Name = updateDto.Name;
            user.Email = updateDto.Email;
            user.Role = updateDto.Role;
            user.Department = updateDto.Department;
            user.Title = updateDto.Title;

            // Update initials if name changed
            var nameParts = updateDto.Name.Split(' ');
            user.Initials = nameParts.Length >= 2 
                ? $"{nameParts[0][0]}{nameParts[1][0]}".ToUpper()
                : nameParts[0].Substring(0, Math.Min(2, nameParts[0].Length)).ToUpper();

            // Update avatar class if role changed
            user.AvatarClass = updateDto.Role switch
            {
                "შემვსები" => "avatar-blue",
                "ხელმძღვანელი" => "avatar-green", // Green
                "შემსრულებელი" => "avatar-orange", // Orange
                "ადმინისტრატორი" => "avatar-red",
                _ => "avatar-blue"
            };

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}
