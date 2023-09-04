namespace forum.Models;

public class Comment
{
    public int CommentId { get; set; }
    public int PostId { get; set; }
    public int? ParentId { get; set; } // ? means nullable
    public List<Comment> Comments { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? LastEdit { get; set; } // ? means nullable
    public int Likes { get; set; }
    public int Dislikes { get; set; }
}