namespace YourProject.API.Models   // REPLACE "YourProject.API" with your actual namespace
{
    public class DigestEntry
    {
        public int DigestEntryID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public int AuthorID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User Author { get; set; } = null!;  // Requires a User model with UserID (int) and Name (string)
    }
}
