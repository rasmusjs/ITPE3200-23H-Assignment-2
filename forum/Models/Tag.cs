namespace forum.Models;

public class Tag
{
    public int TagId { get; set; }

    public string? Name { get; set; }

    // navigation property
    public virtual List<Post>? Posts { get; set; }
}