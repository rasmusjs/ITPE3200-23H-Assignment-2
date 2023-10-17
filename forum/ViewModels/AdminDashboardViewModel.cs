using forum.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace forum.ViewModels;

// Classes and interfaces that are used for rendering HTML elements and layouts 

// View model to pass data from the controller to the view.
// Used by the view to display a forum post and to let the user select category and tags for the posts
public class AdminDashboardViewModel
{
    // List of SelectListItem objects - The different categories that a forum post can belong to
    public IEnumerable<Category> CategoryList { get; set; } = default!;

    // List of SelectListItem objects - The different tags a forum post can have
    public IEnumerable<Tag> TagList { get; set; } = default!;

    public Category? Category { get; set; }
    public Tag? Tag { get; set; }
}