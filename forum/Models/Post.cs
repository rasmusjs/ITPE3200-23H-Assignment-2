using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace forum.Models;

// https://stackoverflow.com/questions/51144399/json-net-attribute-to-ignore-all-properties
[JsonObject(MemberSerialization.OptIn)] // Ignore all the base attributes

// Model for the Post class
public class Post
{
    // Getters and setters for id
    [JsonProperty("PostId")] public int PostId { get; set; }

    // Regex for error handling the post title
    [RegularExpression(@"[0-9a-zA-ZæøåÆØÅ \-/:/?/./!/#]{2,64}",
        ErrorMessage =
            "The title can only contain numbers, letters or characters -:?.!, and must be between 2 to 64 characters.")]
    [Display(Name = "Title")]
    // Getters and setters for post title
    [JsonProperty("Title")]
    public string Title { get; set; } = string.Empty;

    // Regex for error handling the post content

    [StringLength(4096, MinimumLength = 2, ErrorMessage = "The content must be between 2 to 4096 characters.")]
    [Display(Name = "Content")]
    // Getters and setters for post data
    [JsonProperty("Content")]
    public string Content { get; set; } = string.Empty;

    [JsonProperty("TotalLikes")] public int TotalLikes { get; set; }
    [JsonProperty("DateCreated")] public DateTime DateCreated { get; set; }

    [JsonProperty("DateLastEdited")] public DateTime? DateLastEdited { get; set; }

    public string? UserId { get; set; }

    // navigation property
    [JsonProperty("User")] public virtual ApplicationUser? User { get; set; }

    [Required] public int CategoryId { get; set; }

    // navigation property
    [JsonProperty("Category")] public virtual Category? Category { get; set; }

    // navigation property
    [NotMapped] [Required] public virtual List<int>? TagsId { get; set; } // ony used for creating a post

    [NotMapped]
    [JsonProperty("IsLiked")]
    public bool IsLiked { get; set; } // only used for visualizing a post like in the view

    // navigation property
    [JsonProperty("Tags")] public virtual List<Tag>? Tags { get; set; }

    // navigation property
    public virtual List<Comment>? Comments { get; set; }

    [JsonProperty("TotalComments")] public int TotalComments { get; set; }

    // navigation property
    public virtual List<ApplicationUser>? UserLikes { get; set; }
}