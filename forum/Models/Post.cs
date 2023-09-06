namespace forum.Models;

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public int Likes { get; set; } = 0;
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public DateTime? DateLastEdited { get; set; }
    public int UserId { get; set; }
    // navigation property
    public User User { get; set; } = default!;
    /*public Category Category { get; set; } = new()
    {
        CategoryId = new Random().Next(1, 99999),
        Name = "General"
    };*/
    public string Category { get; set; }
    // navigation property
    public List<Tag>? Tags { get; set; }
    // navigation property
    public List<Comment>? Comments { get; set; }
}