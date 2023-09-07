namespace forum.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public DateTime CreationDate { get; set; }
    // navigation property
    //public List<Post>? Posts { get; set; }
    // navigation property
    //public List<Comment>? Comments { get; set; }
    // navigation property
    //public List<Post>? LikedPosts { get; set; }
    // navigation property
    //public List<Comment>? LikedComments { get; set; }
}