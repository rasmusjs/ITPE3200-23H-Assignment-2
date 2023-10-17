using System.Security.Claims;
using forum.DAL;
using forum.Models;
using forum.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace forum.Controllers;

// Controller for the search function
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger; // Ikke satt opp enda!

    // Connect the controller to the different models
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly IForumRepository<Tag> _tagsRepository;

    // Constructor for Dependency Injection to the Data Access Layer from the different repositories
    public HomeController(IForumRepository<Category> categoryRepository,
        IForumRepository<Tag> tagsRepository,
        ILogger<HomeController> logger)
    {
        _categoryRepository = categoryRepository;
        _tagsRepository = tagsRepository;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public string GetUserId()
    {
        //https://stackoverflow.com/questions/29485285/can-not-find-user-identity-getuserid-method
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
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

    // Sends user to index
    public async Task<IActionResult> Index()
    {
        // Fetching the categories and tags data
        IEnumerable<Category>? categories = await _categoryRepository.GetAll();
        IEnumerable<Tag>? tags = await _tagsRepository.GetAll();

        // Exception if there are no tags or categories to show the user
        if (categories == null || tags == null)
            throw new InvalidOperationException("Categories or tags not found, cannot create post");

        // New view model for creating a post
        var adminDashboardViewModel = new AdminDashboardViewModel
        {
            CategoryList = categories,
            TagList = tags
        };

        return View(adminDashboardViewModel);
    }
}