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
    // Connect the controller to the different models
    private readonly IForumRepository<Post> _postRepository;
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly IForumRepository<Tag> _tags;
    private readonly IForumRepository<Comment> _commentRepository;

    private readonly ILogger<PostController> _logger; // Ikke satt opp enda!

    // Constructor for Dependency Injection to the Data Access Layer from the different repositories
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
    
   // Method to refresh the post when user presses the like button
    public IActionResult Refresh()
    {
        return Redirect(Request.Headers["Referer"].ToString());
    }

    // A method to go to post based on id, when the user create or update a post
    public IActionResult GoToPost(int id)
    {
        return RedirectToAction("Post", "Post", new { id });
    }

    // Method for getting card view
    public async Task<IActionResult> Card()
    {
        //Get all posts
        var posts = await GetAllPosts();

        //If no posts found return NotFound
        if (posts == null)
        {
            return NotFound("Item list not found");
        }

        //Update session variable, used for determining which view to use
        HttpContext.Session.SetString("viewModel", "Card");
        return View(new PostsListViewModel(posts, "Card"));
    }

    // Method for getting compact view
    public async Task<IActionResult> Compact()
    {
        //Get all posts
        var posts = await GetAllPosts();

        //If no posts found return NotFound
        if (posts == null)
        {
            return NotFound("Item list not found");
        }

        //Update session variable, used for determining which view to use
        HttpContext.Session.SetString("viewModel", "Compact");
        return View(new PostsListViewModel(posts, "Compact"));
    }
    
    // Method for fetching all posts. Used by Card() and Compact()
    public async Task<IEnumerable<Post>?> GetAllPosts()
    {
        // Get all posts
        var posts = await _postRepository.GetAllPosts();

        // If no posts, return NotFound
        if (posts == null)
        {
            _logger.LogError("[PostController] GetAllPosts failed while executing _itemRepository.GetAll()");
            return null;
        }

        return posts;
    }

    // Method for fetching post by id
    public async Task<IActionResult> Post(int id)
    {
        // Fetch post based on provided id
        var post = await _postRepository.GetTById(id);

        // If no post for the specified id, return NotFound
        if (post == null) return NotFound();

        //Retrieve comments for post
        await _commentRepository.GetCommentsByPostId(id);

        return View(post);
    }

    // Get request for giving the user the view for creating a post 
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var postViewModel = await GetPostViewModel();
        return View(postViewModel);
    }

    // Post request for publishing a post
    [HttpPost]
    public async Task<IActionResult> Create(Post post)
    {
        // Initial values for the post
        post.DateCreated = DateTime.Now;
        post.DateLastEdited = DateTime.Now;
        post.UserId = 1;

        // Assigning tags to the post
        var allTags = await _tags.GetAll();
        if (allTags != null && post.TagsId != null)
            post.Tags = allTags.Where(tag => post.TagsId.Contains(tag.TagId)).ToList();

        // Checks if the post object is valid per the post model and creates the object or return NotFound.
        if (ModelState.IsValid)
        {
            var newPost = await _postRepository.Create(post);

            if (newPost == null) return NotFound("Post not created");

            // Redirects the user to the newly created post
            return GoToPost(newPost.PostId);
        }

        // Returns the user to create post if unsuccessful
        var postViewModel = await GetPostViewModel();
        return View(postViewModel);
    }

    // Method for giving the user the view for creating a post 
    private async Task<PostViewModel> GetPostViewModel()
    {
        // Fetching the categories and tags data
        var categories = await _categoryRepository.GetAll();
        var tags = await _tags.GetAll();

        // Exception if there are no tags or categories to show the user
        if (categories == null || tags == null)
            throw new InvalidOperationException("Categories or tags not found, cannot create post");

        // New view model for creating a post
        var postViewModel = new PostViewModel
        {
            Post = new Post(),
            // Fetching the category select
            CategorySelectList = categories.Select(category => new SelectListItem
            {
                Value = category.CategoryId.ToString(),
                Text = category.Name
            }).ToList(),
            // Fetching the tag list
            TagSelectList = tags.Select(tag => new SelectListItem
            {
                Value = tag.TagId.ToString(),
                Text = tag.Name
            }).ToList()
        };

        // Return the full viewmodel for creating posts to the user
        return postViewModel;
    }

    // Get request for updating posts based on id
    [HttpGet]
    public async Task<IActionResult> Update(int id)
    {
        // Fetching the post based on id
        var post = await _postRepository.GetPostById(id);

        // Fetching categories and tags
        var categories = await _categoryRepository.GetAll();
        var tags = await _tags.GetAll();

        // Error handling if the post is not found
        if (post == null) return NotFound("Post not found, cannot update post");

        // Fetches the tags and checks if the post has category and tags
        var selectedTags = post.Tags;
        if (categories == null || tags == null) return NotFound("Post not found, cannot update post");

        // Creates a new view model
        var postViewModel = new PostViewModel
        {
            Post = post,
            // Fetches the category select list
            CategorySelectList = categories.Select(category => new SelectListItem
            {
                Value = category.CategoryId.ToString(),
                Text = category.Name
            }).ToList(),
            // Fetches the tag select list
            TagSelectList = tags.Select(tag => new SelectListItem
            {
                Value = tag.TagId.ToString(),
                Text = tag.Name,
                Selected = selectedTags != null && selectedTags.Contains(tag)
            }).ToList()
        };

        // Return the full viewmodel for the updated post
        return View(postViewModel);
    }

    // Post request for sending the post update
    [HttpPost]
    public async Task<IActionResult> Update(Post post)
    {
        // Check if model is valid before updating #TODO: Fix this
        if (!ModelState.IsValid) return RedirectToAction(nameof(Update));

        // Remove all the olds tags from post, this is done since we could not find a way to use CASCADE update in EF Core
        if (!await _postRepository.RemoveAllPostTags(post.PostId))
            return NotFound("Post not found, cannot update post");

        /*var postFromDb = await _postRepository.GetTById(post.PostId);
        post.UserId = postFromDb.UserId;
        post.DateCreated = postFromDb.DateCreated;
        post.DateLastEdited = DateTime.Now;*/

        // Adds the required tags again 
        // Source: https://stackoverflow.com/questions/62783700/asp-net-core-razor-pages-select-multiple-items-from-ienumerable-dropdownlist
        // Inspiration for how to get the selected tags
        var allTags = await _tags.GetAll();
        if (allTags != null && post.TagsId != null)
            post.Tags = allTags.Where(tag => post.TagsId.Contains(tag.TagId))
                .ToList(); // Correct way to get the selected tags???

        // Update post
        if (!await _postRepository.Update(post)) return NotFound("Post not found, cannot update post");

        // Sends user back to the updated post 
        return GoToPost(post.PostId);
    }

    // Get request for deleting a post
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        // Fetch the post to be deleted by id. Returns error if not found.
        var post = await _postRepository.GetTById(id);
        if (post == null) return NotFound();
        return View(post);
    }

    // Post request for deleting post and confirming for the user
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Delete post. If post not found, return NotFound
        bool post = await _postRepository.Delete(id); 
        if (post == false) return NotFound();
        
        // Get the current view model from session. Returns card view by default
        string viewModel = HttpContext.Session.GetString("viewModel") ?? "Card"; 
        return RedirectToAction(viewModel, "Post"); // Redirect to the post list
    }

    // Post request for creating a comment
    [HttpPost]
    public async Task<IActionResult> CreateComment(Comment comment)
    {
        // Error handling to check if the model is correct
        if (!ModelState.IsValid) return NotFound("Comment not created, comment invalid");

        // Sets current time to comment and creates comment. Returns NotFound with message if there is no comment
        comment.DateCreated = DateTime.Now;
        var newComment = await _commentRepository.Create(comment);
        if (newComment == null) return NotFound("Comment not created");

        /* Validation not working, fix later */
        return Redirect(
            $"{Url.Action("Post", new { id = newComment.PostId })}#commentId-{newComment.CommentId}"); // Redirect to the post with the comment
    }

    // Post request for updating a comment
    [HttpPost]
    public async Task<IActionResult> UpdateComment(Comment comment)
    {
        // Fetch the comment from database, based on id
        var commentFromDb = await _commentRepository.GetTById(comment.CommentId);

        // Error handling if no comment is found
        if (commentFromDb == null) return NotFound();

        // Checks if the model for comments is valid and returns error message.
        if (!ModelState.IsValid) return NotFound("Comment not updated, comment invalid");

        // Updates the comment in the database
        commentFromDb.DateLastEdited = DateTime.Now;
        commentFromDb.Content = comment.Content;
        await _commentRepository.Update(commentFromDb);

        /* Validation not working, fix later */
        return Redirect(
            $"{Url.Action("Post", new { id = commentFromDb.PostId })}#commentId-{commentFromDb.CommentId}"); // Redirect to the post with the comment
    }

    // Get request for adding likes to a post
    [HttpGet]
    public async Task<IActionResult> LikePost(int id)
    {
        // Fetches post based on id.
        var post = await _postRepository.GetTById(id);

        // Error handling if the post is not found
        if (post == null) return NotFound();

        // Increments like on the post
        post.Likes++;

        // Updates post
        await _postRepository.Update(post);

        // Refreshes the post
        return RedirectToAction(nameof(Refresh));
    }

    // Get request for adding likes to a comment
    [HttpGet]
    public async Task<IActionResult> LikeComment(int id)
    {
        // Fetches comment based on id
        var comment = await _commentRepository.GetTById(id);

        // Error handling if the comment is not found
        if (comment == null) return NotFound();

        // Increments like on the comment
        comment.Likes++;

        // Updates the cimment
        await _commentRepository.Update(comment);

        //return RedirectToAction("Post", "Post", new { id = comment.PostId });
        return Redirect(
            $"{Url.Action("Post", new { id = comment.PostId })}#commentId-{comment.CommentId}"); // Redirect to the post with the comment
    }
}