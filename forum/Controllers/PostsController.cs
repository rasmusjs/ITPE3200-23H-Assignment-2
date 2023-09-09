using System.Linq;
using System.Threading.Tasks;
using forum.DAL;
using Microsoft.AspNetCore.Mvc;
using forum.Models;
using forum.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace forum.Controllers;

public class PostsController : Controller
{
    private readonly IForumRepository<Post> _postRepository;
    private readonly IForumRepository<Tag> _tags;

    private readonly ILogger<PostsController> _logger; // Ikke satt opp enda!

    public PostsController(IForumRepository<Tag> tagRepo, IForumRepository<Post> postRepository,
        ILogger<PostsController> logger)
    {
        _tags = tagRepo;
        _postRepository = postRepository;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Card", "Posts");
    }

    public async Task<IActionResult> Card()
    {
        var posts = await _postRepository.GetAll();
        if (posts == null)
        {
            _logger.LogError("[ItemController] Item list not found while executing _itemRepository.GetAll()");
            return NotFound("Item list not found");
        }

        var postListViewModel = new PostsListViewModel(posts, "Card");
        return View(postListViewModel);
    }

    public async Task<IActionResult> Compact()
    {
        var posts = await _postRepository.GetAll();
        if (posts == null)
        {
            _logger.LogError("[ItemController] Item list not found while executing _itemRepository.GetAll()");
            return NotFound("Item list not found");
        }

        var postListViewModel = new PostsListViewModel(posts, "Compact");
        return View(postListViewModel);
    }

    public async Task<IActionResult> Post(int id)
    {
        var post = await _postRepository.GetTById(id);
        if (post == null)
            return NotFound();
        return View(post);
    }


    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Post post)
    {
        post.DateCreated = DateTime.Now;
        if (ModelState.IsValid)
        {
            await _postRepository.Create(post);
            return RedirectToAction(nameof(Index)); // nameof(Index) keep track of where the use came from
        }

        return View(post);
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var post = await _postRepository.GetTById(id);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> Update(Post post)
    {
        if (ModelState.IsValid)
        {
            await _postRepository.Update(post);
            return RedirectToAction(nameof(Index));
        }

        return View(post);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _postRepository.GetTById(id);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        bool post = await _postRepository.Delete(id);
        if (post == false)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }
}