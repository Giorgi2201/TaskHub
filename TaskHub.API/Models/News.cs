namespace TaskHub.API.Models
{
    public class News
    {
        public int NewsID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int AuthorID { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public User Author { get; set; } = null!;
    }
}
