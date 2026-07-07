using System.Text.Json;

namespace TaskHub.API.DTOs
{
    // Returned when reading one of the current user's drafts. DraftData is left
    // as a raw JSON element (rather than a typed model) so this DTO works for
    // every module's draft shape without needing a module-specific type.
    public class DraftDto
    {
        public string ModuleType { get; set; } = string.Empty;
        public JsonElement DraftData { get; set; }
        public string LastUpdated { get; set; } = string.Empty;
    }

    // Sent by the client to upsert (create or overwrite) its draft for a given
    // module. The module type itself comes from the route, not the body.
    public class SaveDraftDto
    {
        public JsonElement DraftData { get; set; }
    }
}
