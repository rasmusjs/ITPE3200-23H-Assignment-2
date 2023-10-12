using Microsoft.Build.Framework;

namespace forum.Models;

// Model for the Tag class
public class Tag
{
    // Getters and setters for Tag data
    public int TagId { get; set; }

    [Required] public string Name { get; init; } = string.Empty;

    // navigation property
    public virtual List<Post>? Posts { get; set; }
}