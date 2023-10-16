using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace forum.Models;

// Model for the comment class
public class Comment
{
    // Getters and setters for id
    public int CommentId { get; set; }

    // Regex for error handling comment content
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ. ?!\-]{2,512}",
        ErrorMessage = "The comment can contain numbers or letters and be upto 512 characters.")]

    // Getters and setters for data in comments
    public string Content { get; set; } = string.Empty;

    public int TotalLikes { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateLastEdited { get; set; }

    // navigation property
    public int PostId { get; set; }

    public virtual Post? Post { get; set; }

    public string? UserId { get; set; }

    // navigation property
    public virtual ApplicationUser? User { get; set; }

    [NotMapped] public bool IsLiked { get; set; } // only used for visualizing a comment like in the view
    public int? ParentCommentId { get; set; } // Self-referencing foreign key

    // navigation property
    public virtual Comment? ParentComment { get; set; } // Self-referencing navigation property

    public virtual List<Comment>? CommentReplies { get; set; }

    // navigation property
    public virtual List<ApplicationUser>? UserLikes { get; set; }
}