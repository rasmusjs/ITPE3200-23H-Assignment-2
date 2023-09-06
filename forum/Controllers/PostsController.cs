using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using forum.Models;
using forum.ViewModels;

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

    public IActionResult Card()
    {
        List<Post> posts = _postDbContext.Posts.ToList();
        var postListViewModel = new PostsListViewModel(posts, "Card");
        return View(postListViewModel);
    }

    public IActionResult Compact()
    {
        List<Post> posts = _postDbContext.Posts.ToList();
        var postListViewModel = new PostsListViewModel(posts, "Compact");
        return View(postListViewModel);
    }

    public IActionResult Post(int id)
    {
        List<Post> posts = _postDbContext.Posts.ToList();
        var post = posts.FirstOrDefault(i => i.PostId == id);
        if (post == null)
            return NotFound();
        return View(post);
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
post1Comment1.Comments.Add(post1Comment1SubComment1);
var post1Comment2 = new Comment
{
    CommentId = 2,
    Content = "CommentContent2"
};
post1.Comments.Add(post1Comment1);
post1.Comments.Add(post1Comment2);*/