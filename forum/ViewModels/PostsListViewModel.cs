using forum.Models;

// View Model to pass data from controller to the view when displaying a list of forum posts

namespace forum.ViewModels;

public class PostsListViewModel
{
    // Category objects - The category that the forum post belongs to (optional)
    public Category? Categories;

    // The name of the current view
    public string? CurrentViewName;

    // Property with a collection of Post objects (forum posts)
    public IEnumerable<Post> Posts;

    public PostsListViewModel(IEnumerable<Post> posts, string? currentViewName)
    {
        // Initialize the Post property
        Posts = posts;

        // Initialize the CurrentViewName property
        CurrentViewName = currentViewName;
    }
}