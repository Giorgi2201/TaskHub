namespace TaskHub.API.Models
{
    public class Vacancy
    {
        public int VacancyID { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public bool IsActive { get; set; } = true;
        public int AuthorID { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public User Author { get; set; } = null!;
    }
}
