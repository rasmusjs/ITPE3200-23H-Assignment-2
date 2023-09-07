using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using forum.Models;
using forum.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace forum.Controllers;

public class PostsController : Controller
{
    private readonly PostDbContext _postDbContext;

    public PostsController(PostDbContext postDbContext)
    {
        _postDbContext = postDbContext;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Card", "Posts");
    }

    public async Task<IActionResult> Card()
    {
        List<Post> posts = await _postDbContext.Posts.ToListAsync();
        var postListViewModel = new PostsListViewModel(posts, "Card");
        return View(postListViewModel);
    }

    public async Task<IActionResult> Compact()
    {
        List<Post> posts = await _postDbContext.Posts.ToListAsync();
        var postListViewModel = new PostsListViewModel(posts, "Compact");
        return View(postListViewModel);
    }

    public async Task<IActionResult> Post(int id)
    {
        List<Post> posts = await _postDbContext.Posts.ToListAsync();
        var post = posts.FirstOrDefault(i => i.PostId == id);
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
        
        var newUser = new User()
        {
            UserId = new Random().Next(1, 999999),
            Username = "UsernamePlaceholder",
            Email = "EmailPlaceholder@email.com",
            Password = "Password123",
            CreationDate = DateTime.Now,
        };

        post.UserId = newUser.UserId;
        post.User = newUser;
        
        var newCategory = new Category()
        {
            CategoryId = new Random().Next(1, 999999),
            Name = "CategoryPlaceholder"
        };
        
        post.CategoryId = newCategory.CategoryId;
        post.Category = newCategory;

        var newTag1 = new Tag()
        {
            TagId = new Random().Next(1, 999999),
            Name = "TagPlaceholder1"
        };
        var newTag2 = new Tag()
        {
            TagId = new Random().Next(1, 999999),
            Name = "TagPlaceholder2"
        };

        post.Tags = new List<Tag>();
        post.Tags.Add(newTag1);
        post.Tags.Add(newTag2);
        
        //Console.WriteLine(post.DateCreated);
        //Console.WriteLine(postCopy.DateCreated);
        
        //Console.WriteLine("Valid model?");
        //Console.WriteLine(ModelState.IsValid);
        
        //if (ModelState.IsValid)
        if (true)
        {
            _postDbContext.Posts.Add(post);
            await _postDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index)); // nameof(Index) keep track of where the use came from
        }

        return View(post);
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var post = await _postDbContext.Posts.FindAsync(id);
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
            _postDbContext.Posts.Update(post);
            await _postDbContext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(post);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _postDbContext.Posts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        return View(post);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var post = _postDbContext.Posts.Find(id);
        if (post == null)
        {
            return NotFound();
        }

        _postDbContext.Posts.Remove(post);
        await _postDbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /*public List<Post> GetPosts()
    {
        var posts = new List<Post>();

        var post1 = new Post
        {
            PostId = 1,
            Title = "PostTitle1",
            Content =
                "Delicious Italian dish with a thin crust topped with tomato sauce, cheese, and various toppings.",
            Category = new Category()
            {
                CategoryId = 1,
                Name = "Web development"
            },
        };

        var post2 = new Post
        {
            PostId = 2,
            Title = "PostTitle2",
            Content =
                "Delicious Italian dish with a thin crust topped with tomato sauce, cheese, and various toppings.",
            Category = new Category()
            {
                CategoryId = 1,
                Name = "Web development"
            },
        };

        var post3 = new Post
        {
            PostId = 3,
            Title = "PostTitle3",
            Content =
                "Delicious Italian dish with a thin crust topped with tomato sauce, cheese, and various toppings.",
            Category = new Category()
            {
                CategoryId = 1,
                Name = "Web development"
            },
        };

        posts.Add(post1);
        posts.Add(post2);
        posts.Add(post3);

        return posts;
    }*/
}

/*var post1Comment1 = new Comment
{
    CommentId = 1,
    Content = "CommentContent1"
};
var post1Comment1SubComment1 = new Comment
{
    CommentId = 3,
    Content = "SubContent3"
};
post1Comment1.CommentReplies.Add(post1Comment1SubComment1);
var post1Comment2 = new Comment
{
    CommentId = 2,
    Content = "CommentContent2"
};
post1.CommentReplies.Add(post1Comment1);
post1.CommentReplies.Add(post1Comment2);*/