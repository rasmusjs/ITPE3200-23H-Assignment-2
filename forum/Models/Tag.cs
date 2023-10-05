namespace forum.Models;

// Model for the Tag class
public class Tag
{
    // Getters and setters for Tag data
    public int TagId { get; set; }

    public string? Name { get; set; }

    // navigation property
    public virtual List<Post>? Posts { get; set; }
}