using forum.DAL;
using forum.Models;
using forum.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace forum.Controllers;

// Controller for the search function
public class SearchController : Controller
{
    private readonly ILogger<SearchController> _logger; // Ikke satt opp enda!

    // Connect the controller to the different models
    private readonly IForumRepository<Post> _postRepository;

    // Constructor for Dependency Injection to the Data Access Layer from the different repositories
    public SearchController(IForumRepository<Post> postRepository,
        ILogger<SearchController> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    // Function to refresh
    // NOT USED?????
    public IActionResult Refresh()
    {
        return Redirect(Request.Headers["Referer"].ToString());
    }

    // Function to go to post based on id
    public IActionResult GoToPost(int id)
    {
        return RedirectToAction("Post", "Post", new { id });
    }

    // Get request to search based on a provided search term
    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        // Error handling for the search term
        if (string.IsNullOrWhiteSpace(term)) return Refresh(); // TODO: Redirect to error page
        if (term.Length < 2) return Refresh(); // TODO: Redirect to error page

        // Fetch all posts based on the search term
        var posts = await _postRepository.GetAllPostsByTerm(term);

        // Error handling if the term does not provide any posts
        if (posts == null)
        {
            _logger.LogError("[ItemController] Item list not found while executing _itemRepository.GetAll()");
            return NotFound("Item list not found");
        }

        // Return view with all the posts matching the search term
        var postsListViewModel = new PostsListViewModel(posts, "Search");
        return View(postsListViewModel);
    }

    // Sends user to index
    public IActionResult Index()
    {
        return View();
    }
}