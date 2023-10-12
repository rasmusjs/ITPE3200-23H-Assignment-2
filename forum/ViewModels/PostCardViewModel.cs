using forum.Models;

namespace forum.ViewModels;

public class PostCardViewModel
{
    public Post Post { get; set; } = null!;
    public bool LimitContent { get; set; }
}