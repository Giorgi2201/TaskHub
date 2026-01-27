namespace TaskHub.API.Models
{
    public class Status
    {
        public int StatusID { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusClass { get; set; } = string.Empty;

        // Navigation property
        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}
