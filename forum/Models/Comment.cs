using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace forum.Models;

// Model for the comment class
public class Comment
{
    // Getters and setters for id
    public int CommentId { get; set; }

    // Getters and setters for data in comments

    [StringLength(512, MinimumLength = 2, ErrorMessage = "The content must be between 2 to 512 characters.")]
    public string Content { get; set; } = string.Empty; // if set to empty, the comment is "deleted"

    public int TotalLikes { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? DateLastEdited { get; set; }

    // navigation property
    public int PostId { get; set; }

    [JsonIgnore] public virtual Post? Post { get; set; }

    [JsonIgnore] public string? UserId { get; set; }

    // navigation property
    [JsonIgnore] public virtual ApplicationUser? User { get; set; }
    [NotMapped] [JsonProperty("UserName")] public string? UserName { get; set; }


    [NotMapped] public bool IsLiked { get; set; } // only used for visualizing a comment like in the view
    public int? ParentCommentId { get; set; } // Self-referencing foreign key

    // navigation property
    public virtual Comment? ParentComment { get; set; } // Self-referencing navigation property

    public virtual List<Comment>? CommentReplies { get; set; }

    // navigation property
    public virtual List<ApplicationUser>? UserLikes { get; set; }
}