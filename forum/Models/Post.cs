namespace forum.Models;

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; } = 0;
    public DateTime DateCreated { get; set; }
    public DateTime? DateLastEdited { get; set; }

    public int UserId { get; set; }

    // navigation property
    public User User { get; set; } = default!;

    public int CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    // navigation property
    public virtual List<Tag>? Tags { get; set; }

    // navigation property
    public virtual List<Comment> Comments { get; set; } = default!;
}