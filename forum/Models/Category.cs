using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    public string Name { get; set; } = string.Empty;

    // Getters and setters for category color
    [Display(Name = "Color")] public string Color { get; set; } = string.Empty;

    // Getters and setters for category PicturePath
    [Display(Name = "URL")] public string? PicturePath { get; set; } = string.Empty;
    [NotMapped] public byte[]? PictureBytes { get; set; } // used for file upload
}