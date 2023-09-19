using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace forum.Models;

// Model for the Post class
public class Post
{
    // Getters and setters for id
    public int PostId { get; set; }

    // Regex for error handling the post title
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ. \-]{2,64}",
        ErrorMessage = "The title must be numbers or letters and between 2 to 64 characters.")]
    [Display(Name = "Title")]
    
    // Getters and setters for post title
    public string Title { get; set; }

    // Regex for error handling the post content
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ. \-]{2,1024}",
        ErrorMessage = "The content can contain numbers or letters and be upto 1024 characters.")]
    [Display(Name = "Content")]
    
    // Getters and setters for post data
    public string Content { get; set; }
    public int Likes { get; set; } = 0;
    public DateTime DateCreated { get; set; }
    public DateTime? DateLastEdited { get; set; }
    public int? UserId { get; set; }
    // navigation property
    public virtual User? User { get; set; } = default!;
    [Required] public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }
    // navigation property
    [NotMapped] [Required] public virtual List<int>? TagsId { get; set; } // ony used for creating a post
    // navigation property
    public virtual List<Tag>? Tags { get; set; }
    // navigation property
    public virtual List<Comment>? Comments { get; set; }
}