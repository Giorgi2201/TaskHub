namespace TaskHub.API.Models
{
    public class RequestParticipant
    {
        public int RequestParticipantID { get; set; }
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public string Role { get; set; } = string.Empty; // e.g., "შემვსები", "ხელმძღვანელი", "შემსრულებელი"

        // Navigation properties
        public Request Request { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
