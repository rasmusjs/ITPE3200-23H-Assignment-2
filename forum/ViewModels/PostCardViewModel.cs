using forum.Models;

namespace forum.ViewModels;

// ViewModel used on PostCard (partial view) to conditionally change it's appearance and functionality instead of creating multiple, nearly identical, partial views.
public class PostCardViewModel
{
    public PostCardViewModel(Post post, bool limitContent = false, bool hideGoToPost = false)
    {
        // Initialize the Post property
        Post = post;

        // Initialize the LimitContent property
        LimitContent = limitContent;

        // Initialize the HideGoToPost property
        HideGoToPost = hideGoToPost;
    }

    public Post Post { get; set; }

    // Limits the amount of visible content and places a shadow above overflowing text
    public bool LimitContent { get; set; }

    // Removes the "Go to post" button
    public bool HideGoToPost { get; set; }
}