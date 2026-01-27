namespace TaskHub.API.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Subcategory> Subcategories { get; set; } = new List<Subcategory>();
        public ICollection<Request> Requests { get; set; } = new List<Request>();
    }
}
