using System.Security.Claims;
using forum.DAL;
using forum.Models;
using forum.ViewModels;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace forum.Controllers;

public class PostController : Controller
{
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly IForumRepository<Comment> _commentRepository;


    private readonly ILogger<PostController> _logger; // Ikke satt opp enda!

    // Connect the controller to the different models
    private readonly IForumRepository<Post> _postRepository;
    private readonly IForumRepository<Tag> _tags;
    private readonly UserManager<ApplicationUser> _userManager;

    // Constructor for Dependency Injection to the Data Access Layer from the different repositories
    public PostController(IForumRepository<Category> categoryRepository,
        IForumRepository<Tag> tagRepo, IForumRepository<Post> postRepository,
        IForumRepository<Comment> commentRepository,
        UserManager<ApplicationUser> userManager,
        ILogger<PostController> logger)
    {
        _categoryRepository = categoryRepository;
        _tags = tagRepo;
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _userManager = userManager;
        _logger = logger;
    }


    [HttpGet]
    [Authorize]
    public string GetUserId()
    {
        //https://stackoverflow.com/questions/29485285/can-not-find-user-identity-getuserid-method
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
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


    [HttpGet]
    // Method for getting card view
    public async Task<IActionResult> Card(string sortby = "")
    {
        //Get all posts
        var posts = await GetAllPosts(sortby);

        //If no posts found return NotFound
        if (posts == null) return NotFound("Item list not found");

        //Update session variable, used for determining which view to use
        HttpContext.Session.SetString("viewModel", "Card");
        return View(new PostsListViewModel(posts, "Card"));
    }

    [HttpGet]
    // Method for getting compact view
    public async Task<IActionResult> Compact(string sortby = "")
    {
        //Get all posts
        var posts = await GetAllPosts(sortby);

        //If no posts found return NotFound
        if (posts == null) return NotFound("Item list not found");

        //Update session variable, used for determining which view to use
        HttpContext.Session.SetString("viewModel", "Compact");
        return View(new PostsListViewModel(posts, "Compact"));
    }

    // Method for fetching all posts. Used by Card() and Compact()
    public async Task<IEnumerable<Post>?> GetAllPosts(string sortby = "")
    {
        // Get all posts
        var posts = await _postRepository.GetAllPosts();


        // If no posts, return NotFound
        if (posts == null)
        {
            _logger.LogError("[PostController] GetAllPosts failed while executing _itemRepository.GetAll()");
            return null;
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
    [Authorize]
    public async Task<IActionResult> Create()
    {
        var postViewModel = await GetPostViewModel();
        return View(postViewModel);
    }

    // Post request for publishing a post
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(Post post)
    {
        // Initial values for the post
        post.DateCreated = DateTime.Now;
        post.DateLastEdited = DateTime.Now;
        post.UserId = GetUserId();

        // Assigning tags to the post
        var allTags = await _tags.GetAll();
        if (allTags != null && post.TagsId != null)
            post.Tags = allTags.Where(tag => post.TagsId.Contains(tag.TagId)).ToList();

        // Sanitizing the post content
        post.Content = new HtmlSanitizer().Sanitize(post.Content);
        // This is done to prevent XSS attacks. Source: https://weblog.west-wind.com/posts/2018/Aug/31/Markdown-and-Cross-Site-Scripting

        // Checks if the post object is valid per the post model and creates the object or return NotFound.
        if (ModelState.IsValid)
        {
            var newPost = await _postRepository.Create(post);

            if (newPost == null) return NotFound("Post not created");

            // Fetches the user
            var user = await _userManager.FindByIdAsync(post.UserId);
            user.Posts ??= new List<Post>();
            user.Posts.Add(newPost);
            // Updates the user attribute
            await _userManager.UpdateAsync(user);

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
    [Authorize]
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
    [Authorize]
    public async Task<IActionResult> Update(Post post)
    {
        var postViewModel = await GetPostViewModel();


        if (post.UserId != GetUserId()) // Checks if the user is the owner of the post, // TODO: Add error message
            return View(postViewModel); // Checks if the user is the owner of the post


        // Sanitizing the post content
        post.Content = new HtmlSanitizer().Sanitize(post.Content);

        // Check if model is valid before updating #TODO: Fix this
        if (!ModelState.IsValid)
            // Returns the user to create post if unsuccessful
            return View(postViewModel);


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
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        // Fetch the post to be deleted by id. Returns error if not found.
        var post = await _postRepository.GetTById(id);
        if (post == null) return NotFound();

        if (post.UserId != GetUserId()) // Checks if the user is the owner of the post, // TODO: Add error message
            // If the user is not the owner of the post, return to the post
            return RedirectToAction("Post", "Post", new { id });

        // Return the post to be deleted
        return View(post);
    }

    // Post request for deleting post and confirming for the user
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var post = await _postRepository.GetTById(id);
        if (post == null) return NotFound();

        if (post.UserId != GetUserId()) // Checks if the user is the owner of the post, // TODO: Add error message
            return RedirectToAction("Post", "Post", new { id }); // Send user back to the post if not owner

        // Delete post. If post not found, return NotFound
        var confimedDeleted = await _postRepository.Delete(id);
        if (confimedDeleted == false) // TODO: Add error message
            return NotFound();

        // Get the current view model from session. Returns Card view by default
        var viewModel = HttpContext.Session.GetString("viewModel") ?? "Card";
        return RedirectToAction(viewModel, "Post"); // Redirect to the post list
    }

    // Post request for creating a comment
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateComment(Comment comment)
    {
        // Error handling to check if the model is correct
        if (!ModelState.IsValid) // TODO: Add error message
            return Redirect(
                $"{Url.Action("Post", new { id = comment.PostId })}"); // Redirect to the post with the comment

        // Sets current time to comment and creates comment. Returns NotFound with message if there is no comment
        comment.DateCreated = DateTime.Now;
        comment.UserId = GetUserId();


        var newComment = await _commentRepository.Create(comment);

        if (newComment == null)
        {
            Console.Write("Could not create comment");
            return Redirect(
                $"{Url.Action("Post", new { id = comment.PostId })}"); // Redirect to the post with the comment
        }

        // Fetches the user
        var user = await _userManager.FindByIdAsync(comment.UserId);

        user.Comments ??= new List<Comment>();
        user.Comments.Add(newComment);
        // Updates the user attribute
        await _userManager.UpdateAsync(user);


        /* Validation not working, fix later */
        return Redirect(
            $"{Url.Action("Post", new { id = newComment.PostId })}#commentId-{newComment.CommentId}"); // Redirect to the post with the comment
    }

    // Post request for updating a comment
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> UpdateComment(Comment comment)
    {
        // Fetch the comment from database, based on id
        var commentFromDb = await _commentRepository.GetTById(comment.CommentId);

        // Error handling if no comment is found
        if (commentFromDb == null) // TODO: Add error message
            return Redirect(
                $"{Url.Action("Post", new { id = comment.PostId })}#commentId-{comment.CommentId}"); // Redirect to the post with the comment

        // Checks if the user is the owner of the comment
        if (commentFromDb.UserId != GetUserId()) // TODO: Add error message
            return Redirect(
                $"{Url.Action("Post", new { id = comment.PostId })}#commentId-{comment.CommentId}"); // Redirect to the post with the comment

        // Checks if the model for comments is valid and returns error message.
        if (!ModelState.IsValid) // TODO: Add error message
            return Redirect(
                $"{Url.Action("Post", new { id = commentFromDb.PostId })}#commentId-{commentFromDb.CommentId}"); // Redirect to the post with the comment

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
    [Authorize]
    public async Task<IActionResult> LikePost(int id)
    {
        // Fetches post based on id
        var post = await _postRepository.GetTById(id);

        // Error handling if the post is not found
        if (post == null) return RedirectToAction(nameof(Refresh)); // TODO: Add error message

        // Fetches the user
        var user = await _userManager.FindByIdAsync(GetUserId());

        // Error handling if the user is not found
        if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

        // Checks if the user has already liked the post
        if (user.LikedPosts != null && user.LikedPosts.Any(t => t.PostId == id))
        {
            post.TotalLikes--; // Decrements like on the post
            user.LikedPosts.Remove(post); // Removes the post from the user's liked posts
            await _userManager.UpdateAsync(user); // Updates the user
            await _postRepository.Update(post); // Updates the post
            return RedirectToAction(nameof(Refresh)); // Refreshes the post
        }

        // Increments like on the post
        post.TotalLikes++;

        // Adds the post to the user's liked posts
        user.LikedPosts ??= new List<Post>();
        user.LikedPosts.Add(post);

        // Updates the user attribute
        await _userManager.UpdateAsync(user);

        // Updates the post
        await _postRepository.Update(post);

        // Refreshes the post
        return RedirectToAction(nameof(Refresh));
    }

    // Get request for adding likes to a comment
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> LikeComment(int id)
    {
        // Fetches comment based on id
        var comment = await _commentRepository.GetTById(id);

        // Error handling if the comment is not found
        if (comment == null) return NotFound(); // TODO: Add error message

        // Fetches the user
        var user = await _userManager.FindByIdAsync(GetUserId());

        // Error handling if the user is not found
        if (user == null) return RedirectToPage("/Account/Login", new { area = "Identity" });

        // Checks if the user has already liked the comment
        if (user.LikedComments != null && user.LikedComments.Any(t => t.CommentId == id))
        {
            comment.TotalLikes--; // Decrements like on the comment
            user.LikedComments.Remove(comment); // Removes the comment from the user's liked comments
            await _userManager.UpdateAsync(user); // Updates the user
            await _commentRepository.Update(comment); // Updates the comment
            return Redirect(
                $"{Url.Action("Post", new { id = comment.PostId })}#commentId-{comment.CommentId}"); // Redirect to the post with the comment
        }

        // Increments like on the comment
        comment.TotalLikes++;

        // Adds the comment to the user's liked comments
        user.LikedComments ??= new List<Comment>();
        user.LikedComments.Add(comment);

        // Updates the user attribute
        await _userManager.UpdateAsync(user);

        // Updates the comment
        await _commentRepository.Update(comment);

        // Refreshes the post
        return Redirect(
            $"{Url.Action("Post", new { id = comment.PostId })}#commentId-{comment.CommentId}"); // Redirect to the post with the comment
    }
    // Get request for adding likes to a comment

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id)
    {
        // Fetch the comment from database, based on id
        var commentFromDb = await _commentRepository.GetTById(id);

        // Error handling if no comment is found
        if (commentFromDb == null) // TODO: Add error message
            return Refresh();

        // Checks if the user is the owner of the comment
        if (commentFromDb.UserId != GetUserId()) // TODO: Add error message
            return Redirect(
                $"{Url.Action("Post", new { id = commentFromDb.PostId })}#commentId-{id}"); // Redirect to the post with the comment

        // Checks if the model for comments is valid and returns error message.
        if (!ModelState.IsValid)
            return Redirect(
                $"{Url.Action("Post", new { id = commentFromDb.PostId })}#commentId-{commentFromDb.CommentId}"); // Redirect to the post with the comment

        // Updates the comment in the database
        commentFromDb.DateLastEdited = DateTime.Now;
        commentFromDb.Content = "[Deleted]";
        await _commentRepository.Update(commentFromDb);

        /* Validation not working, fix later */
        return Redirect(
            $"{Url.Action("Post", new { id = commentFromDb.PostId })}"); // Redirect to the post
    }
}