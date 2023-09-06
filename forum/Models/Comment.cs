namespace forum.Models;

public class Comment
{
    public int CommentId { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; } = 0;
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? LastEdit { get; set; }

    public int UserId { get; set; }

    // navigation property
    public User User { get; set; } = default!;

    // navigation property
    public int? ParentCommentId { get; set; } // Self-referencing foreign key
    public Comment? CommentParent { get; set; } // Self-referencing navigation property
    public virtual List<Comment>? CommentReplies { get; set; }
}