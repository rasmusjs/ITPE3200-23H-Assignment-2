using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;

namespace forum.Models;

// https://stackoverflow.com/questions/51144399/json-net-attribute-to-ignore-all-properties
[JsonObject(MemberSerialization.OptIn)] // Ignore all the base attributes from the IdentityUser class such as password
// Model for the User class
public class ApplicationUser : IdentityUser
{
    // Get the value from the base class
    [JsonProperty("UserName")] [NotMapped] public override string UserName => base.UserName;

    public DateTime CreationDate { get; set; } = DateTime.Now;
    [JsonProperty("ProfilePicture")] public byte[]? ProfilePicture { get; set; }

    // navigation property
    public virtual List<Post>? Posts { get; set; }

    // navigation property
    public virtual List<Comment>? Comments { get; set; }

    // navigation property
    public virtual List<Post>? LikedPosts { get; set; }

    // navigation property
    public virtual List<Comment>? LikedComments { get; set; }
}