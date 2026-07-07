namespace TaskHub.API.Models
{
    // Generic "minimized" draft, one row per (user, module). Replaces the old
    // module-specific DigestDraft table with a single reusable table so new
    // modules (Vacancies, News, ...) don't each need their own draft table,
    // migration, and controller.
    //
    // DESIGN CHOICE: draft content is stored as a single JSON blob
    // (DraftDataJson) rather than as strongly-typed columns per module.
    // Draft data is transient scratch state - it exists only so an in-progress
    // form survives a minimize/refresh, and it is never queried, filtered, or
    // reported on independently of "does this user have a draft for this
    // module". Strongly-typed columns would require a new migration every time
    // a module's form gains/loses a field (and a full new table for every new
    // module), for no real benefit here. A JSON payload lets the frontend's
    // existing form-data shape be persisted as-is and keeps this table stable
    // as modules evolve or new ones are added.
    public class Draft
    {
        public int DraftId { get; set; }

        // Owning user.
        public int UserId { get; set; }

        // Which module this draft belongs to, e.g. "digest", "vacancy", "news".
        // Combined with UserId via a unique index (configured in
        // TaskHubDbContext.OnModelCreating) so a user can have at most one
        // draft per module, independently of their drafts for other modules.
        public string ModuleType { get; set; } = string.Empty;

        // The draft's form data, serialized as JSON. Shape is whatever the
        // owning module's frontend form looks like - this table doesn't need
        // to know or care about individual fields.
        public string DraftDataJson { get; set; } = "{}";

        // Bumped on every upsert so callers can tell how fresh the draft is.
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        public User User { get; set; } = null!;
    }

    // Known module type values. Not a database enum (kept as a plain string
    // column so adding a new module never requires a migration) but centralized
    // here to avoid typos scattered across controller/frontend code.
    public static class DraftModuleTypes
    {
        public const string Digest = "digest";
        public const string Vacancy = "vacancy";
        public const string News = "news";

        public static readonly string[] All = { Digest, Vacancy, News };
    }
}
