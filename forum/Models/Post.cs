namespace forum.Models;

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; } = 0;

    // Dates
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public DateTime? DateLastEdited { get; set; }

    // Relations
    public User User { get; set; }
    public Category Category { get; set; }
    public List<Tag> Tags { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}