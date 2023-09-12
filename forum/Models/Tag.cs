namespace forum.Models;

public class Tag
{
    public int TagId { get; set; }

    public string? Name { get; set; }

    // navigation property
    public List<Post>? Posts { get; set; }
}