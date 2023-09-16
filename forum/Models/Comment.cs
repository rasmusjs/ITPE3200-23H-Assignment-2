using System.ComponentModel.DataAnnotations;

namespace forum.Models;

public class Comment
{
    public int CommentId { get; set; }

    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ. \-]{2,512}",
        ErrorMessage = "The comment can contain numbers or letters and be upto 512 characters.")]
    public string Content { get; set; } = string.Empty;

    public int Likes { get; set; } = 0;
    public DateTime DateCreated { get; set; }
    public DateTime? DateLastEdited { get; set; }


    // navigation property
    public int PostId { get; set; }
    public virtual Post? Post { get; set; }

    /*
    public int UserId { get; set; }

    // navigation property
    public User User { get; set; } = default!;*/

    // navigation property
    public int? ParentCommentId { get; set; } // Self-referencing foreign key
    public virtual Comment? ParentComment { get; set; } // Self-referencing navigation property
    public virtual List<Comment>? CommentReplies { get; set; }
}