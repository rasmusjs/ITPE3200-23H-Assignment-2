namespace forum.Models;

public class Comment
{
    public int CommentId { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; } = 0;

    // Dates
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? LastEdit { get; set; } // ? means nullable

    // Relations
    public int? UserId { get; set; }
    public int? PostId { get; set; }
    public int? ParentId { get; set; }
    public List<Comment> Comments { get; set; } = new();
}