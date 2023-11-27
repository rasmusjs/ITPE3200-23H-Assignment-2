using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace forum.Models;

// https://stackoverflow.com/questions/51144399/json-net-attribute-to-ignore-all-properties
[JsonObject(MemberSerialization.OptIn)] // Ignore all the base attributes 

// Model for the comment class
public class Comment
{
    // Getters and setters for id
    [JsonProperty("commentId")] public int CommentId { get; set; }

    // Getters and setters for data in comments

    [StringLength(512, MinimumLength = 2, ErrorMessage = "The content must be between 2 to 512 characters.")]
    [JsonProperty("content")]
    public string Content { get; set; } = string.Empty; // if set to empty, the comment is "deleted"

    [JsonProperty("totalLikes")] public int TotalLikes { get; set; }
    [JsonProperty("dateCreated")] public DateTime DateCreated { get; set; }
    [JsonProperty("dateLastEdited")] public DateTime? DateLastEdited { get; set; }

    // navigation property
    [JsonProperty("postId")] public int PostId { get; set; }

    public virtual Post? Post { get; set; }

    public string? UserId { get; set; }

    // navigation property
    [JsonProperty("user")] public virtual ApplicationUser? User { get; set; }
    [JsonProperty("ParentCommentId")] public int? ParentCommentId { get; set; } // Self-referencing foreign key

    // navigation property
    public virtual Comment? ParentComment { get; set; } // Self-referencing navigation property

    [JsonProperty("commentReplies")] public virtual List<Comment>? CommentReplies { get; set; }

    // navigation property
    public virtual List<ApplicationUser>? UserLikes { get; set; }
    
    // navigation property
    public virtual List<ApplicationUser>? SavedByUsers { get; set; }
    
}