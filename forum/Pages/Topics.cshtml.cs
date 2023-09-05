using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace forum.Pages;

public class TopicsModel : PageModel
{
    private readonly ILogger<TopicsModel> _logger;

    public TopicsModel(ILogger<TopicsModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        
    }
}