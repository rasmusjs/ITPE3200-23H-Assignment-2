using forum.Models;

namespace forum.ViewModels;

using Microsoft.AspNetCore.Mvc.Rendering;

public class PostCreateViewModel
{
    public Post? Post { get; set; } = default!;
    public List<SelectListItem> CategorySelectList { get; set; } = default!;

    public List<SelectListItem> TagSelectList { get; set; } = default!;
}