using System.ComponentModel.DataAnnotations;

namespace forum.Models;

public class Category
{
    public int CategoryId { get; set; }

    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ. \-]{2,1024}",
        ErrorMessage = "The name can contain numbers or letters and be upto 1024 characters.")]
    [Display(Name = "Category name")]
    public string Name { get; set; }
}