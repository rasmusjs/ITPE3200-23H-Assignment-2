using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using forum.DAL;
using Microsoft.AspNetCore.Mvc;
using forum.Models;
using forum.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace forum.Controllers;

public class PostsController : Controller
{
    private readonly IForumRepository<Post> _postRepository;
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly IForumRepository<Tag> _tags;

    private readonly ILogger<PostsController> _logger; // Ikke satt opp enda!

    public PostsController(IForumRepository<Category> categoryRepository,
        IForumRepository<Tag> tagRepo, IForumRepository<Post> postRepository,
        ILogger<PostsController> logger)
    {
        _categoryRepository = categoryRepository;
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


    public async Task<IEnumerable?> GetAllPosts()
    {
        var posts = await _postRepository.GetAll();
        var category = await _categoryRepository.GetAll();
        if (posts == null || category == null)
        {
            _logger.LogError("[ItemController] Item list not found while executing _itemRepository.GetAll()");
            return null;
        }


        return posts;
    }

    public async Task<IActionResult> Post(int id)
    {
        var post = await _postRepository.GetTById(id);
        if (post == null)
            return NotFound();
        return View(post);
    }


    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var categories = await _categoryRepository.GetAll();

        var tags = await _tags.GetAll();

        var postCreateViewModel = new PostCreateViewModel
        {
            Post = new Post(),

            CategorySelectList = categories.Select(category => new SelectListItem
            {
                Value = category.CategoryId.ToString(),
                Text = category.Name
            }).ToList(),

            TagSelectList = tags.Select(tag => new SelectListItem
            {
                Value = tag.TagId.ToString(),
                Text = tag.Name
            }).ToList()
        };

        return View(postCreateViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Post post)
    {
        post.DateCreated = DateTime.Now;
        post.DateLastEdited = DateTime.Now;
        post.UserId = 1;

        Console.WriteLine("Valid" + ModelState.IsValid);


        //Check https://stackoverflow.com/questions/62783700/asp-net-core-razor-pages-select-multiple-items-from-ienumerable-dropdownlist
        // for how to get the selected tags

        if (post.Tags != null)
            foreach (var tag in post.Tags)
            {
                Console.WriteLine("Tag: " + tag.Name);
            }
        else
        {
            Console.WriteLine("Tags is null!");
        }


        /*if (ModelState.IsValid)
        {*/
        await _postRepository.Create(post);
        return RedirectToAction(nameof(Index)); // nameof(Index) keep track of where the use came from

        /*
        }

        return View(post);
        return RedirectToAction(nameof(Create));*/
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