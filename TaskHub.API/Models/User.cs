namespace TaskHub.API.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
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
    }
}
