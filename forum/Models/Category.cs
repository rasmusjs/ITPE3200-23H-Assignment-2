using System.ComponentModel.DataAnnotations;

namespace forum.Models;

// Model for the Category class
public class Category
{
    // Getters and setters for id
    public int CategoryId { get; set; }

    // Regex for error handling category names
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ. \-]{2,1024}",
        ErrorMessage = "The name can contain numbers or letters and be upto 1024 characters.")]
    [Display(Name = "Category name")]

    // Getters and setters for category name
    public string Name { get; set; }
}