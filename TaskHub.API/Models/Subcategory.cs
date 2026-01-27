namespace TaskHub.API.Models
{
    public class Subcategory
    {
        public int SubcategoryID { get; set; }
        public string SubcategoryName { get; set; } = string.Empty;
        public int CategoryID { get; set; }

        // Navigation properties
        public Category Category { get; set; } = null!;
        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}
