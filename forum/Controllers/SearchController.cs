using System.Security.Claims;
using forum.DAL;
using forum.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace forum.Controllers;

// Controller for the search function
[ApiController]
[Route("api/[controller]")]
public class SearchController : Controller
{
    private readonly ILogger<SearchController> _logger;

    // Connect the controller to the different models
    private readonly IForumRepository<Post> _postRepository;

    // Constructor for Dependency Injection to the Data Access Layer from the different repositories
    public SearchController(IForumRepository<Post> postRepository,
        ILogger<SearchController> logger)
    {
        _postRepository = postRepository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public string GetUserId()
    {
        //https://stackoverflow.com/questions/29485285/can-not-find-user-identity-getuserid-method
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
    
    [HttpGet("{term}/{sortby=newest}")] // Set a default string
    public async Task<IActionResult> NewSearch(string term, string sortby = "")
    {
        // Error handling for the search term
        if (string.IsNullOrWhiteSpace(term) || term.Length < 2)
            return BadRequest("Search term must be at least 2 characters long");

        // Fetch all posts based on the search term
        var posts = await _postRepository.GetAllPostsByTerm(term, GetUserId());

        // Error handling if the term does not provide any posts
        if (posts == null)
        {
            _logger.LogInformation("[Search controller] Search(), No posts found for search term: " + term);
            // Return view with all the posts matching the search term
            return NotFound("No posts found for search term");
        }

        posts = sortby switch
        {
            "newest" => posts.OrderByDescending(post => post.DateCreated),
            "oldest" => posts.OrderBy(post => post.DateCreated),
            "likes" => posts.OrderByDescending(post => post.TotalLikes),
            "leastlikes" => posts.OrderBy(post => post.TotalLikes),
            "comments" => posts.OrderByDescending(post => post.Comments!.Count),
            "leastcomments" => posts.OrderBy(post => post.Comments!.Count),
            _ => posts.OrderByDescending(post => post.DateCreated)
        };

        // Return view with all the posts matching the search term
        return Ok(posts);
    }
    
}