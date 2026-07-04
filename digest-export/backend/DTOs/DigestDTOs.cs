namespace YourProject.API.DTOs   // REPLACE "YourProject.API" with your actual namespace
{
    public class DigestEntryDto
    {
        public int DigestEntryID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public string PeriodFrom { get; set; } = string.Empty;
        public string PeriodTo { get; set; } = string.Empty;
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

    public class CreateDigestEntryDto
    {
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
    }

    public class UpdateDigestEntryDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string SourceName { get; set; } = string.Empty;
        public string SourceUrl { get; set; } = string.Empty;
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
    }
}
