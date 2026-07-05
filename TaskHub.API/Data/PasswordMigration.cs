using Microsoft.AspNetCore.Identity;
using TaskHub.API.Models;

namespace TaskHub.API.Data
{
    // ─────────────────────────────────────────────────────────────────────────
    // ONE-TIME DATA FIX: hashes any plaintext passwords still sitting in the
    // Users table.
    //
    // WHY THIS EXISTS:
    // The `Users` table originally stored passwords as plaintext (both rows
    // created by hand and the rows baked into the EF Core migration's seed data
    // via HasData — see TaskHubDbContext.SeedData). Login now verifies passwords
    // with PasswordHasher<User> instead of a raw string comparison, so every
    // existing plaintext password needs to be converted to a proper hash before
    // anyone can log in again.
    //
    // HOW IT WORKS / WHY IT'S SAFE TO RUN ON EVERY STARTUP:
    // - It reads every user, and for each one checks whether the stored
    //   `Password` value already looks like a PasswordHasher output
    //   (see LooksLikeHashedPassword). Anything that doesn't is treated as
    //   legacy plaintext and is re-hashed in place.
    // - Already-hashed passwords are left completely untouched, so running this
    //   again after the first fix (e.g. on every app restart) is a cheap no-op.
    //   This makes it safe to call unconditionally from Program.cs rather than
    //   requiring a separate manual "run this once" step.
    // - This only ever tightens security (plaintext -> hash); it never weakens
    //   or resets a password that's already hashed.
    // ─────────────────────────────────────────────────────────────────────────
    public static class PasswordMigration
    {
        public static void HashPlaintextPasswords(TaskHubDbContext context)
        {
            var hasher = new PasswordHasher<User>();

            // Load all users into memory first: we need to inspect/rewrite the
            // Password column itself, so this can't be expressed as a SQL query.
            var usersWithPlaintextPasswords = context.Users
                .ToList()
                .Where(u => !LooksLikeHashedPassword(u.Password))
                .ToList();

            if (usersWithPlaintextPasswords.Count == 0)
            {
                return;
            }

            foreach (var user in usersWithPlaintextPasswords)
            {
                user.Password = hasher.HashPassword(user, user.Password);
            }

            context.SaveChanges();
        }

        // Heuristic: PasswordHasher<T> always outputs a base64 string whose
        // decoded bytes start with a one-byte format marker (0x00 = legacy V2,
        // 0x01 = V3, the current default) and are comfortably longer than any
        // realistic plaintext password's raw byte length. Legacy plaintext
        // values (e.g. "password1", "admin123") are neither valid base64 of a
        // suitable length nor start with that marker byte, so they're correctly
        // identified as "not a hash" and get hashed by the caller.
        private static bool LooksLikeHashedPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            byte[] decoded;
            try
            {
                decoded = Convert.FromBase64String(password);
            }
            catch (FormatException)
            {
                return false;
            }

            const int minimumHashLength = 15; // real hashes are 61+ bytes; this is a generous floor
            return decoded.Length >= minimumHashLength && (decoded[0] == 0x00 || decoded[0] == 0x01);
        }
    }
}
