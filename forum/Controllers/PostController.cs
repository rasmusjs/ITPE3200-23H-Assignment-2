using System.ComponentModel;
using forum.DAL;
using Microsoft.AspNetCore.Mvc;
using forum.Models;
using forum.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace forum.Controllers;

public class PostController : Controller
{
    private readonly IForumRepository<Post> _postRepository;
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly IForumRepository<Tag> _tags;
    private readonly IForumRepository<Comment> _commentRepository;

    private readonly ILogger<PostController> _logger; // Ikke satt opp enda!

    public PostController(IForumRepository<Category> categoryRepository,
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

    public IActionResult Index()
    {
        return RedirectToAction("Card", "Post");
    }

    public async Task<IActionResult> Card()
    {
        var posts = await GetAllPosts();

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
        var posts = await GetAllPosts();
        if (posts == null)
        {
            _logger.LogError("[ItemController] Item list not found while executing _itemRepository.GetAll()");
            return NotFound("Item list not found");
        }

        var postListViewModel = new PostsListViewModel(posts, "Compact");
        return View(postListViewModel);
    }


    public async Task<IEnumerable<Post>?> GetAllPosts()
    {
        var posts = await _postRepository.GetAllPosts();
        if (posts == null)
        {
            _logger.LogError("[PostController] GetAllPosts failed while executing _itemRepository.GetAll()");
            return null;
        }

        return posts;
    }

    public async Task<IActionResult> Post(int id)
    {
        var post = await _postRepository.GetTById(id);
        var comments = await _commentRepository.GetCommentsByPostId(id);
        if (post == null)
            return NotFound();

        //post.Comments = (List<Comment>?)comments;


        /*// Display comments and replies
        foreach (var comment in comments)
        {
            Console.WriteLine($"Comment: {comment.Content}");
            foreach (var reply in comment.CommentReplies)
            {
                Console.WriteLine($"  Reply: {reply.Content}");
            }
        }*/

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


        //Check https://stackoverflow.com/questions/62783700/asp-net-core-razor-pages-select-multiple-items-from-ienumerable-dropdownlist
        // for how to get the selected tags
        var allTags = await _tags.GetAll();
        post.Tags = allTags.Where(tag => post.TagsId.Contains(tag.TagId))
            .ToList(); // Correct way to get the selected tags???


        if (ModelState.IsValid)
        {
            await _postRepository.Create(post);
            return RedirectToAction(nameof(Index)); // nameof(Index) keep track of where the use came from
        }

        //return View(post);
        return RedirectToAction(nameof(Create));
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
    
    [HttpPost]
    public async Task<IActionResult> CreateReply(Comment replyComment)
    {
        // Writes all properties of replyComment
        foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(replyComment))
        {
            string name = descriptor.Name;
            object value = descriptor.GetValue(replyComment);
            Console.WriteLine("{0}={1}", name, value);
        }

        /*
        TODO:
        Once the replyComment contains the proper information/content, store it in the database. 
        */

        if (ModelState.IsValid)
        {
            await _commentRepository.Create(replyComment);
        }
        
        return RedirectToAction(nameof(Index));
    }
    
    [HttpPost]
    public async Task<IActionResult> UpdateComment(Comment comment)
    {
        if (ModelState.IsValid)
        {
            await _commentRepository.Update(comment);
        }
        
        return RedirectToAction(nameof(Index));
    }
}