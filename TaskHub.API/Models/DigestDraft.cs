namespace TaskHub.API.Models
{
    // Stores a single in-progress ("minimized") digest draft per user so the
    // draft survives page refreshes. This is intentionally a separate table
    // from DigestEntry and has no impact on published digest entries.
    public class DigestDraft
    {
        public int DigestDraftID { get; set; }

        // Owning user. A unique index on this column (configured in
        // TaskHubDbContext.OnModelCreating) guarantees at most one draft per user.
        public int UserID { get; set; }

        // Draft field values mirror the digest form. All are nullable/optional
        // because a draft can be saved half-filled while the user is editing.
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? SourceName { get; set; }
        public string? SourceUrl { get; set; }
        public DateTime? PeriodFrom { get; set; }
        public DateTime? PeriodTo { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;

        // Bumped on every upsert so callers can tell how fresh the draft is.
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;
    }
}
