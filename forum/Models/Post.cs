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
    //public User User { get; set; }
    public Category Category { get; set; } = new()
    {
        CategoryId = new Random().Next(1, 9999),
        Name = "General"
    };

    public List<Tag> Tags { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
}