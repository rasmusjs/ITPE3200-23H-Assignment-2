namespace forum.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }

    // Date
    public DateTime CreationDate { get; set; } = DateTime.Now;

    // Relations
    public List<Post> Posts { get; set; }
    public List<Comment> Comments { get; set; }
    public List<Post> LikedPosts { get; set; }
    public List<Comment> LikedComments { get; set; }
}