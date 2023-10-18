using forum.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace forum.ViewModels;

// Classes and interfaces that are used for rendering HTML elements and layouts 

// View model to pass data from the controller to the view.
// Used by the view to display a forum post and to let the user select category and tags for the posts
public class DashboardViewModel
{
    // List of CategoryList objects - The different categories that a forum post can belong to
    public IEnumerable<Category> CategoryList { get; set; } = default!;

    // List of TagList objects - The different tags a forum post can have
    public IEnumerable<Tag> TagList { get; set; } = default!;

    // Used to send in new categories to the database
    public Category? Category { get; set; }

    // Used to send in new tags to the database
    public Tag? Tag { get; set; }
}