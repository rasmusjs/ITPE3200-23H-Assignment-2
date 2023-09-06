namespace forum.Models;

public class Category
{
    public int CategoryId { get; set; }
    public string Name { get; set; }
    public List<SubCategory> SubCategories { get; set; } = new();
}