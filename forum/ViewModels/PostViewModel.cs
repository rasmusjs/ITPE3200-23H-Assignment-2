using forum.Models;

namespace forum.ViewModels;

using Microsoft.AspNetCore.Mvc.Rendering;

public class PostViewModel
{
    // Property which contains a Post object (forum post)
    public Post? Post { get; set; } = default!;
    // List of SelectListItem objects - The different categories that a forum post can belong to
    public List<SelectListItem> CategorySelectList { get; set; } = default!;
    // List of SelectListItem objects - The different tags a forum post can have
    public List<SelectListItem> TagSelectList { get; set; } = default!;
}