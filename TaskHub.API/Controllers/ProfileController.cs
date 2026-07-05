using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskHub.API.Data;
using TaskHub.API.DTOs;
using TaskHub.API.Models;

namespace TaskHub.API.Controllers
{
    // Backs the user profile page: read-only profile info + the change-password
    // flow. Kept separate from AuthController (login/session bootstrapping) and
    // UsersController (admin-only user management of OTHER users) since this is
    // specifically "a user acting on their own account".
    //
    // Auth follows the same convention as the rest of the API (AuthController,
    // DigestDraftsController, etc.): there is no token/JWT/session layer, so the
    // acting user's id is passed explicitly by the client as a route parameter,
    // exactly like DigestDraftsController's {userId} routes.
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private const int MinimumPasswordLength = 8;

        private readonly TaskHubDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public ProfileController(TaskHubDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        // GET: api/profile/5
        // Read-only info for display on the profile page (name/email/title/
        // department/role). Never includes the password hash.
        [HttpGet("{userId}")]
        public async Task<ActionResult<ProfileDto>> GetProfile(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserID == userId);

            if (user == null)
            {
                return NotFound(new { message = "მომხმარებელი ვერ მოიძებნა" });
            }

            return Ok(new ProfileDto
            {
                Name = user.Name,
                Email = user.Email,
                Title = user.Title,
                Department = user.Department,
                Role = user.Role
            });
        }

        // PUT: api/profile/5/password
        // Changes the given user's password. Every failure case below returns a
        // distinct message so the frontend can surface specific, actionable
        // feedback rather than one generic error.
        [HttpPut("{userId}/password")]
        public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserID == userId);
            if (user == null)
            {
                return NotFound(new { message = "მომხმარებელი ვერ მოიძებნა" });
            }

            // 1. The old password must match what's currently hashed in the DB.
            if (!VerifyPassword(user, dto.OldPassword))
            {
                return BadRequest(new { field = "oldPassword", message = "ძველი პაროლი არასწორია" });
            }

            // 2. New password and confirmation must match. (The frontend should
            //    already validate this, but we never trust the client alone.)
            if (dto.NewPassword != dto.ConfirmNewPassword)
            {
                return BadRequest(new { field = "confirmNewPassword", message = "ახალი პაროლები არ ემთხვევა ერთმანეთს" });
            }

            // 3. Minimum strength/length. No stronger convention exists elsewhere
            //    in the app, so this defaults to a sensible minimum of 8 characters.
            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < MinimumPasswordLength)
            {
                return BadRequest(new
                {
                    field = "newPassword",
                    message = $"ახალი პაროლი უნდა შედგებოდეს მინიმუმ {MinimumPasswordLength} სიმბოლოსგან"
                });
            }

            // 4. The new password must not be identical to the old one.
            if (VerifyPassword(user, dto.NewPassword))
            {
                return BadRequest(new { field = "newPassword", message = "ახალი პაროლი არ უნდა იყოს ძველის იდენტური" });
            }

            // All checks passed — hash and persist the new password.
            user.Password = _passwordHasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "პაროლი წარმატებით შეიცვალა" });
        }

        // Mirrors AuthController.VerifyPassword: guards against FormatException so
        // a non-hash value in the DB fails verification cleanly instead of throwing.
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
