namespace TaskHub.API.Models
{
    public class Comment
    {
        public int CommentID { get; set; }
        public int RequestID { get; set; }
        public int UserID { get; set; }
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public Request Request { get; set; } = null!;
        public User User { get; set; } = null!;
    }
}
