namespace TaskHub.API.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Stores the ASP.NET Core Identity password hash (via PasswordHasher<User>),
        // never plaintext. Any legacy plaintext values still present in the database
        // (e.g. from the original EF Core seed data) are hashed in place on startup —
        // see PasswordMigration.HashPlaintextPasswords in Program.cs.
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string AvatarClass { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Request> InitiatedRequests { get; set; } = new List<Request>();
        public ICollection<RequestParticipant> Participations { get; set; } = new List<RequestParticipant>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<DigestEntry> DigestEntries { get; set; } = new List<DigestEntry>();
    }
}
