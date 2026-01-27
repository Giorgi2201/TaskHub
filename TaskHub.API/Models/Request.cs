namespace TaskHub.API.Models
{
    public class Request
    {
        public int RequestID { get; set; }
        public int CategoryID { get; set; }
        public int SubcategoryID { get; set; }
        public string Description { get; set; } = string.Empty;
        public int StatusID { get; set; }
        public int InitiatorID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Category Category { get; set; } = null!;
        public Subcategory Subcategory { get; set; } = null!;
        public Status Status { get; set; } = null!;
        public User Initiator { get; set; } = null!;
        public ICollection<RequestParticipant> Participants { get; set; } = new List<RequestParticipant>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
