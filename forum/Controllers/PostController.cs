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


    public IActionResult Refresh()
    {
        return Redirect(Request.Headers["Referer"].ToString());
    }

    public IActionResult GoToPost(int id)
    {
        return RedirectToAction("Post", "Post", new { id });
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

        if (post == null)
            return NotFound();

        //Retrieve comments for post
        await _commentRepository.GetCommentsByPostId(id);

        return View(post);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var postCreateViewModel = await GetPostCreateViewModel();
        return View(postCreateViewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Post post)
    {
        post.DateCreated = DateTime.Now;
        post.DateLastEdited = DateTime.Now;
        post.UserId = 1;

        var allTags = await _tags.GetAll();
        if (allTags != null && post.TagsId != null)
            post.Tags = allTags.Where(tag => post.TagsId.Contains(tag.TagId)).ToList();

        if (ModelState.IsValid)
        {
            var newPost = await _postRepository.Create(post);

            if (newPost == null)
            {
                return NotFound("Post not created");
            }

            return GoToPost(newPost.PostId);
        }

        var postCreateViewModel = await GetPostCreateViewModel();
        return View(postCreateViewModel);
    }

    private async Task<PostCreateViewModel> GetPostCreateViewModel()
    {
        var categories = await _categoryRepository.GetAll();
        var tags = await _tags.GetAll();

        if (categories != null && tags != null)
        {
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

            return postCreateViewModel;
        }

        throw new InvalidOperationException("Categories or tags not found, cannot create post");
    }


    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        var post = await _postRepository.GetPostById(id);

        var categories = await _categoryRepository.GetAll();
        var tags = await _tags.GetAll();

        if (post != null)
        {
            var selectedTags = post.Tags;
            if (categories != null && tags != null)
            {
                var postCreateViewModel = new PostCreateViewModel
                {
                    Post = post,
                    CategorySelectList = categories.Select(category => new SelectListItem
                    {
                        Value = category.CategoryId.ToString(),
                        Text = category.Name
                    }).ToList(),

                    TagSelectList = tags.Select(tag => new SelectListItem
                    {
                        Value = tag.TagId.ToString(),
                        Text = tag.Name,
                        Selected = selectedTags != null && selectedTags.Contains(tag)
                    }).ToList()
                };

                return View(postCreateViewModel);
            }
        }

        return NotFound("Post not found, cannot update post");
    }

    [HttpPost]
    public async Task<IActionResult> Update(Post post)
    {
        // Check if model is valid before updating
        if (!ModelState.IsValid) return RedirectToAction(nameof(Create));


        // Remove all the olds tags from post, this is done since I could not find a way to use CASCADE update in EF Core
        if (!await _postRepository.RemoveAllPostTags(post.PostId))
            return NotFound("Post not found, cannot update post");

        /*var postFromDb = await _postRepository.GetTById(post.PostId);
        post.UserId = postFromDb.UserId;
        post.DateCreated = postFromDb.DateCreated;
        post.DateLastEdited = DateTime.Now;*/


        //Check https://stackoverflow.com/questions/62783700/asp-net-core-razor-pages-select-multiple-items-from-ienumerable-dropdownlist
        // for how to get the selected tags
        var allTags = await _tags.GetAll();
        if (allTags != null && post.TagsId != null)
            post.Tags = allTags.Where(tag => post.TagsId.Contains(tag.TagId))
                .ToList(); // Correct way to get the selected tags???

        // Update post
        if (!await _postRepository.Update(post)) return NotFound("Post not found, cannot update post");


        return GoToPost(post.PostId);
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

        return RedirectToAction("Card", "Post");
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment(Comment comment)
    {
        if (ModelState.IsValid)
        {
            comment.DateCreated = DateTime.Now;
            var newComment = await _commentRepository.Create(comment);
            if (newComment == null)
            {
                return NotFound("Comment not created");
            }

            /* Validation not working, fix later */
            return Redirect(
                $"{Url.Action("Post", new { id = newComment.PostId })}#commentId-{newComment.CommentId}"); // Redirect to the post with the comment
        }

        return NotFound("Comment not created, comment invalid");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateComment(Comment comment)
    {
        var commentFromDb = await _commentRepository.GetTById(comment.CommentId);

        if (commentFromDb == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            commentFromDb.DateLastEdited = DateTime.Now;
            commentFromDb.Content = comment.Content;
            await _commentRepository.Update(commentFromDb);
        }

        /* Validation not working, fix later */
        return Redirect(
            $"{Url.Action("Post", new { id = commentFromDb.PostId })}#commentId-{commentFromDb.CommentId}"); // Redirect to the post with the comment
    }


    [HttpGet]
    public async Task<IActionResult> LikePost(int id)
    {
        var post = await _postRepository.GetTById(id);

        if (post == null)
        {
            return NotFound();
        }

        post.Likes++;

        await _postRepository.Update(post);

        return RedirectToAction(nameof(Refresh));
    }

    [HttpGet]
    public async Task<IActionResult> LikeComment(int id)
    {
        var comment = await _commentRepository.GetTById(id);

        if (comment == null)
        {
            return NotFound();
        }

        comment.Likes++;

        await _commentRepository.Update(comment);

        //return RedirectToAction("Post", "Post", new { id = comment.PostId });
        return Redirect(
            $"{Url.Action("Post", new { id = comment.PostId })}#commentId-{comment.CommentId}"); // Redirect to the post with the comment
    }
}