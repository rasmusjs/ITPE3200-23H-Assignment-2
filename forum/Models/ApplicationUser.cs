using Microsoft.AspNetCore.Identity;

namespace forum.Models;

// Model for the User class
public class ApplicationUser : IdentityUser
{
    public DateTime CreationDate { get; set; } = DateTime.Now;
    public byte[]? ProfilePicture { get; set; } = null!;

    // navigation property
    public virtual List<Post>? Posts { get; set; }

    // navigation property
    public virtual List<Comment>? Comments { get; set; }
    // navigation property
    //public List<Post>? LikedPosts { get; set; }
    // navigation property
    //public List<Comment>? LikedComments { get; set; }
}