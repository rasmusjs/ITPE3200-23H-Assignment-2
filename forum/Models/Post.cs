using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace forum.Models;

// Model for the Post class
public class Post
{
    // Getters and setters for id
    public int PostId { get; set; }

    // Regex for error handling the post title
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ.?! \-]{2,64}",
        ErrorMessage = "The title can only contain numbers, letters or !?, and must be between 2 to 64 characters.")]
    [Display(Name = "Title")]

    // Getters and setters for post title
    public string Title { get; set; }

    // Regex for error handling the post content

    [StringLength(4096, MinimumLength = 2, ErrorMessage = "The content must be between 2 to 4096 characters.")]
    [Display(Name = "Content")]
    // Getters and setters for post data
    public string Content { get; set; }

    public int Likes { get; set; } = 0;
    public DateTime DateCreated { get; set; }
    public DateTime? DateLastEdited { get; set; }

    public string? UserId { get; set; }

    // navigation property
    public virtual ApplicationUser? User { get; set; }
    [Required] public int CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    // navigation property
    [NotMapped] [Required] public virtual List<int>? TagsId { get; set; } // ony used for creating a post

    // navigation property
    public virtual List<Tag>? Tags { get; set; }

    // navigation property
    public virtual List<Comment>? Comments { get; set; }
}