namespace forum.Models;

public class Post
{
    public int PostId { get; set; }
    public User User { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public DateTime? LastEdit { get; set; } // ? means nullable
    public Topic Topic { get; set; }
    public List<Tag> Tags { get; set; }
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public List<Comment> Comments { get; set; }
}