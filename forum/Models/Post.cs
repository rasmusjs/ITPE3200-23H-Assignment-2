using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace forum.Models;

public class Post
{
    public int PostId { get; set; }

    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ. \-]{2,64}",
        ErrorMessage = "The Name must be numbers or letters and between 2 to 64 characters.")]
    [Display(Name = "Post name")]
    public string Title { get; set; }

    public string Content { get; set; }
    public int Likes { get; set; } = 0;
    public DateTime DateCreated { get; set; }
    public DateTime? DateLastEdited { get; set; }

    public int? UserId { get; set; }

    // navigation property
    public virtual User? User { get; set; } = default!;

    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    [NotMapped] public virtual List<int>? TagsId { get; set; } // ony used for creating a post

    // navigation property
    public virtual List<Tag>? Tags { get; set; }

    // navigation property
    public virtual List<Comment>? Comments { get; set; }
}