using System.ComponentModel;
using forum.DAL;
using Microsoft.AspNetCore.Mvc;
using forum.Models;
using forum.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace forum.Controllers;

public class SearchController : Controller
{
    private readonly IForumRepository<Post> _postRepository;
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly IForumRepository<Tag> _tags;
    private readonly IForumRepository<Comment> _commentRepository;

    private readonly ILogger<PostController> _logger; // Ikke satt opp enda!


    public SearchController(IForumRepository<Category> categoryRepository,
        IForumRepository<Tag> tagRepo, IForumRepository<Post> postRepository,
        IForumRepository<Comment> commentRepository,
        ILogger<PostController> logger)
    {
        _categoryRepository = categoryRepository;
        _tags = tagRepo;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _logger = logger;
    }


    public IActionResult Refresh()
    {
        return Redirect(Request.Headers["Referer"].ToString());
    }

    public IActionResult GoToPost(int id)
    {
        return RedirectToAction("Post", "Post", new { id });
    }


    [HttpGet]
    public async Task<IActionResult> Search(string term)
    {
        if (string.IsNullOrWhiteSpace(term)) return Index(); // TODO: Redirect to error page
        if (term.Length < 2) return Index(); // TODO: Redirect to error page


        var posts = await _postRepository.GetAllPostsByTerm(term);
        if (posts == null)
        {
            _logger.LogError("[ItemController] Item list not found while executing _itemRepository.GetAll()");
            return NotFound("Item list not found");
        }

        var postsListViewModel = new PostsListViewModel(posts, "Search");
        return View(postsListViewModel);
    }

    public IActionResult Index()
    {
        return View();
    }
}