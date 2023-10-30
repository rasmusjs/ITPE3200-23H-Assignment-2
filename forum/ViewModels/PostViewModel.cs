using forum.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace forum.ViewModels;

// Classes and interfaces that are used for rendering HTML elements and layouts 

// View model to pass data from the controller to the view.
// Used by the view to display a forum post and to let the user select category and tags for the posts
public class PostViewModel
{
    // Property which contains a Post object (forum post)
    public Post Post { get; set; }

    // List of SelectListItem objects - The different categories that a forum post can belong to
    public List<SelectListItem> CategorySelectList { get; set; }

    // List of SelectListItem objects - The different tags a forum post can have
    public List<SelectListItem> TagList { get; set; }

    public PostViewModel(Post post, List<SelectListItem> categorySelectList, List<SelectListItem> tagList)
    {
        // Initialize the Post property
        Post = post;

        // Initialize the CategorySelectList property
        CategorySelectList = categorySelectList;

        // Initialize the TagList property
        TagList = tagList;
    }
}