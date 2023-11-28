using System.Security.Claims;
using forum.DAL;
using forum.Models;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace forum.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostController : Controller
{
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly IForumRepository<Comment> _commentRepository;
    private readonly ForumDbContext _forumDbContext;
    private readonly ILogger<PostController> _logger;
    private readonly IMemoryCache _memoryCache;

    // Connect the controller to the different models
    private readonly IForumRepository<Post> _postRepository;
    private readonly IForumRepository<Tag> _tags;
    private readonly UserManager<ApplicationUser> _userManager;

    // Constructor for Dependency Injection to the Data Access Layer from the different repositories
    public PostController(IForumRepository<Category> categoryRepository,
        IForumRepository<Tag> tagsRepository, IForumRepository<Post> postRepository,
        ForumDbContext forumDbContext,
        IForumRepository<Comment> commentRepository,
        UserManager<ApplicationUser> userManager, IMemoryCache memoryCache,
        ILogger<PostController> logger)
    {
        _categoryRepository = categoryRepository;
        _tags = tagsRepository;
        _postRepository = postRepository;
        _forumDbContext = forumDbContext;
        _commentRepository = commentRepository;
        _userManager = userManager;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    /*[HttpGet]
    //[Authorize]
    public string GetUserId()
    {
        // This is needed to see if the user actually is exist in the database
        if (_userManager.GetUserAsync(User).Result != null)
            //https://stackoverflow.com/questions/29485285/can-not-find-user-identity-getuserid-method
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        return "";
    }*/

    // Get request to fetch user identity
    [HttpGet]
    public string GetUserId()
    {
        //https://stackoverflow.com/questions/29485285/can-not-find-user-identity-getuserid-method
        return User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
    }

    // Method to check if the user is admin
    public bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    // Method for fetching all posts. Used by Card() and Compact()
    [HttpGet("posts/{sortby=newest}")] // Set a default string
    public async Task<IActionResult> GetAllPosts(string sortby = "")
    {
        // Source for cashing is taken from https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory?view=aspnetcore-6.0

        // Define a cache key
        var cacheKey = "AllPosts";

        var posts = null as IEnumerable<Post>;
        // Try to get the data from the cache
        if (_memoryCache.TryGetValue(cacheKey, out IEnumerable<Post>? cachedPosts))
        {
            Console.WriteLine("Using cached data");
            posts = cachedPosts; // If the data is in the cache, use it
        }

        // If the data is not in the cache, fetch it from the database
        if (posts == null) posts = await _postRepository.GetAllPosts(GetUserId());

        // If no posts, return NotFound
        if (posts == null || !posts.Any())
        {
            _logger.LogError("[PostController] GetAllPosts failed while executing GetAllPosts()");
            return NotFound("No posts found");
        }

        // Convert the IEnumerable to an array
        posts = posts.ToArray();

        // Add the data to the cache with a specified cache duration
        _memoryCache.Set(cacheKey, posts, TimeSpan.FromMinutes(15));

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

        return Ok(posts);
    }

    // Method for fetching post by id
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPost(int id)
    {
        // Fetch post based on provided id
        var post = await _postRepository.GetPostById(id, GetUserId());

        // If no post for the specified id, return NotFound
        if (post == null)
        {
            _logger.LogError("[PostController] Post failed, while executing GetPostById()");
            return NotFound("Post not found, cannot show post");
        }

        return Ok(post);
    }


    // Get request for giving the user the view for creating a post 
    [HttpGet("GetTags")]
    public async Task<IActionResult> GetTags()
    {
        // Get all tags
        var tags = await _tags.GetAll();
        // If tags is null return not found
        if (tags == null || !tags.Any()) return NotFound("Tags not found");
        // Return
        return Ok(tags);
    }


    // Get request for giving the user the view for creating a post 
    [HttpGet("GetCategories")]
    public async Task<IActionResult> GetCategories()
    {
        // Get all categories
        var categories = await _categoryRepository.GetAll();
        // If tags is null return not found
        if (categories == null || !categories.Any()) return NotFound("Categories not found");
        // Return
        return Ok(categories);
    }

    // Get request for giving the user the view for creating a post 
    [HttpGet("GetComments/{id:int}")]
    public async Task<IActionResult> GetComments(int id)
    {
        // Get all categories
        var comments = await _commentRepository.GetAllCommentsByPostId(id);
        // If tags is null return not found
        if (comments == null || !comments.Any()) return NotFound("Comments not found");
        // Return
        return Ok(comments);
    }


    // Post request for publishing a post
    [HttpPost("CreatePost")]
    public async Task<IActionResult> NewCreate(Post post)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        // Set initial values for the post
        post.DateCreated = DateTime.Now;
        post.DateLastEdited = DateTime.Now;
        post.UserId = GetUserId(); // Assigning user to the post 
        post.Category = await _categoryRepository.GetTById(post.CategoryId); // Assigning category to the post

        // Assigning tags to the post
        var allTags = await _tags.GetAll();
        if (allTags != null && post.TagsId != null)
            post.Tags = allTags.Where(tag => post.TagsId.Contains(tag.TagId)).ToList();

        // Sanitizing the post content
        post.Content = new HtmlSanitizer().Sanitize(post.Content);
        // This is done to prevent XSS attacks. Source: https://weblog.west-wind.com/posts/2018/Aug/31/Markdown-and-Cross-Site-Scripting

        // Checks if the post object is valid per the post model and creates the object or return NotFound.
        if (!ModelState.IsValid) return StatusCode(422, "Post not valid, cannot create post");

        // Try to create the post
        var newPost = await _postRepository.Create(post);

        // If the post is not created, return 422 Unprocessable Content
        if (newPost == null)
        {
            _logger.LogError("[PostController] Create failed, while executing Create()");
            return StatusCode(422, "Post not created successfully");
        }

        // Fetches the user
        var user = await _userManager.FindByIdAsync(post.UserId);

        // Checks if the user is null
        if (user == null)
        {
            _logger.LogError("[PostController] Create failed, while executing Create(). No user");
            return StatusCode(422, "User not found");
        }

        // If the user has no posts, create a new list of posts
        user.Posts ??= new List<Post>();
        // Adds the post to the user's posts
        user.Posts.Add(newPost);

        // Attempt to update the user attribute
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            _logger.LogError("[PostController] Create failed, while executing Create(). Update user failed");
            return StatusCode(500, "Error occurred while updating user data");
        }

        // Remove the cached data
        _memoryCache.Remove("AllPosts");

        // Redirects the user to the newly created post
        return Ok(newPost.PostId);
    }


    // Post request for sending the post update
    [HttpPost("UpdatePost")]
    public async Task<IActionResult> NewUpdate(Post post)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        // Sanitizing the post content
        post.Content = new HtmlSanitizer().Sanitize(post.Content);

        // Check if model is valid before updating
        if (!ModelState.IsValid || post.TagsId == null)
            return StatusCode(500, "Internal server error while updating post please try again");

        // Fetch the post from database, based on id needed to update
        var postFromDb = await _postRepository.GetTById(post.PostId);

        if (postFromDb == null)
        {
            _logger.LogError("[Post controller] Update failed, GetPostById() returned null");
            return NotFound("Post not found, cannot update post");
        }

        // Checks if the user is the owner of the post
        if (!GetUserId().Equals(postFromDb.UserId))
            return StatusCode(403, "You are not the owner of the post");

        // Detach the entity from the DbContext to avoid tracking conflicts
        _forumDbContext.Entry(postFromDb).State = EntityState.Detached;
        // Source: https://stackoverflow.com/questions/48202403/instance-of-entity-type-cannot-be-tracked-because-another-instance-with-same-key

        // Remove all the old tags from the post
        await _postRepository.RemoveAllPostTags(postFromDb.PostId);

        // Adds the required tags again
        var allTags = await _tags.GetAll();

        if (allTags == null)
        {
            _logger.LogError("[Post controller] Update failed, GetAll() for tags returned null");
            return NotFound("Tags not found, cannot update post");
        }

        // Link tags to post
        postFromDb.Tags = allTags.Where(tag => post.TagsId.Contains(tag.TagId)).ToList();

        // Update post
        postFromDb.DateLastEdited = DateTime.Now;
        postFromDb.Title = post.Title;
        postFromDb.Content = post.Content;
        postFromDb.CategoryId = post.CategoryId;

        // Update post
        if (!await _postRepository.Update(postFromDb))
        {
            _logger.LogError("[PostController] Update failed, while executing Update()");
            return StatusCode(500, "Internal server error while updating post please try again");
        }

        // Remove the cached data
        _memoryCache.Remove("AllPosts");

        // Sends the user back to the updated post
        return Ok(post.PostId);
    }


    // Post request for deleting post and confirming for the user
    [HttpGet("DeletePost/{id:int}")]
    public async Task<IActionResult> NewDeleteConfirmed(int id)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        var post = await _postRepository.GetTById(id);
        if (post == null)
        {
            _logger.LogError("[PostController] DeleteConfirmed failed, failed while executing GetTById()");
            return NotFound("Post not found, cannot delete post");
        }

        // Checks if the user is the owner of the post
        if (post.UserId != GetUserId() && !IsAdmin())
        {
            _logger.LogError("[PostController] DeleteConfirmed failed, user is not the owner of the comment");
            return StatusCode(403, "You are not the owner of the post");
        }

        // Delete post. If post not found, return NotFound
        var confirmedDeleted = await _postRepository.Delete(id);
        if (confirmedDeleted == false)
        {
            _logger.LogError("[PostController] DeleteConfirmed failed, failed while executing Delete()");
            return StatusCode(500, "Internal server error while deleting post please try again");
        }

        // Remove the cached data
        _memoryCache.Remove("AllPosts");

        return Ok("Post deleted successfully");
    }


    // Post request for creating a comment
    [HttpPost("CreateComment")]
    public async Task<IActionResult> NewCreateComment(Comment comment)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again");

        // Sanitizing the post content
        comment.Content = new HtmlSanitizer().Sanitize(comment.Content);

        // Error handling to check if the model is correct
        if (!ModelState.IsValid) return StatusCode(422, "Could not create the comment, the comment is not valid");

        // Sets current time to comment
        comment.DateCreated = DateTime.Now;
        comment.UserId = GetUserId();

        // Creates the comment
        var newComment = await _commentRepository.Create(comment);

        if (newComment == null) return StatusCode(500, "Internal server error while creating comment please try again");


        // Fetches the post to update the total comments
        var post = await _postRepository.GetTById(comment.PostId);

        // Error handling if the post is not found
        if (post == null)
        {
            _logger.LogError("[PostController] LikePost failed, failed while executing GetTById() returned null");
            return NotFound("Post not found, cannot like post");
        }

        // Increments the total comments on the post
        post.TotalComments++;

        // Updates the post
        if (!await _postRepository.Update(post))
        {
            _logger.LogError("[PostController] CreatePost failed, failed while executing Update()");
            return StatusCode(500, "Internal server error while updating post please try again");
        }


        // Fetches the user
        var user = await _userManager.FindByIdAsync(comment.UserId);

        // If the user has no comments, create a new list of comments, else add the comment to the user's comments
        user.Comments ??= new List<Comment>();
        user.Comments.Add(newComment);

        // Updates the users comments
        await _userManager.UpdateAsync(user);

        // Remove the cached data
        _memoryCache.Remove("AllPosts");

        return Ok("Created comment successfully");
    }


    // Post request for updating a comment
    [HttpPost("UpdateComment")]
    public async Task<IActionResult> NewUpdateComment(Comment comment)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        // Sanitizing the post content
        comment.Content = new HtmlSanitizer().Sanitize(comment.Content);

        // Checks if the model for comments is valid and returns error message.
        if (!ModelState.IsValid)
            return StatusCode(422,
                "Could not update the comment, the comment is not valid"); // 422 Unprocessable Entity


        // Fetch the comment from database, based on id
        var commentFromDb = await _commentRepository.GetTById(comment.CommentId);

        // Error handling if no comment is found
        if (commentFromDb == null)
        {
            _logger.LogError("[PostController] UpdateComment failed, while executing GetTById() returned null");
            return NotFound("Comment not found, cannot update comment");
        }

        // Checks if the user is the owner of the comment
        if (commentFromDb.UserId != GetUserId())
        {
            _logger.LogError("[PostController] UpdateComment failed, user is not the owner of the comment");
            return StatusCode(403, "You are not the owner of the comment");
        }


        // Updates the comment in the database
        commentFromDb.DateLastEdited = DateTime.Now;
        commentFromDb.Content = comment.Content;
        if (!await _commentRepository.Update(commentFromDb))
        {
            _logger.LogError("[PostController] UpdateComment failed, failed while executing Update()");
            return StatusCode(500, "Internal server error while updating comment please try again");
        }


        return Ok("Comment updated successfully");
    }


    // Get request for adding likes to a post
    [HttpGet("LikePost/{id:int}")]
    public async Task<IActionResult> NewLikePost(int id)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        // Fetches post based on id
        var post = await _postRepository.GetTById(id);

        // Error handling if the post is not found
        if (post == null)
        {
            _logger.LogError("[PostController] LikePost failed, failed while executing GetTById() returned null");
            return NotFound("Post not found, cannot like post");
        }

        // Fetches the user
        var user = await _userManager.FindByIdAsync(GetUserId());

        // Error handling if the user is not found
        if (user == null)
        {
            _logger.LogError(
                "[PostController] LikePost failed, failed while executing _userManager.FindByIdAsync(GetUserId()) returned null");
            return NotFound("User not found, cannot like post. Please log in again");
        }

        // Checks if the user has already liked the post
        if (user.LikedPosts != null && user.LikedPosts.Any(t => t.PostId == id))
        {
            post.TotalLikes--; // Decrements like on the post
            user.LikedPosts.Remove(post); // Removes the post from the user's liked posts
            await _userManager.UpdateAsync(user); // Updates the user
            if (!await _postRepository.Update(post)) // Updates the post
            {
                _logger.LogError("[PostController] LikePost failed, failed while executing Update()");
                return StatusCode(500, "Internal server error while updating post please try again");
            }

            return Ok("Post unliked successfully");
        }

        // Increments like on the post
        post.TotalLikes++;

        // If the user has liked posts, create a new list of posts, else add the post to the user's liked posts
        user.LikedPosts ??= new List<Post>();
        user.LikedPosts.Add(post);

        // Updates the user attribute
        await _userManager.UpdateAsync(user);

        // Updates the post
        if (!await _postRepository.Update(post))
        {
            _logger.LogError("[PostController] LikePost failed, failed while executing Update()");
            return StatusCode(500, "Internal server error while updating post please try again");
        }


        return Ok("Post liked successfully");
    }

    // Get request for adding saves to a post
    [HttpGet("SavePost/{id:int}")]
    public async Task<IActionResult> SavePost(int id)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        // Fetches post based on id
        var post = await _postRepository.GetTById(id);

        // Error handling if the post is not found
        if (post == null)
        {
            _logger.LogError("[PostController] SavePost failed, failed while executing GetTById() returned null");
            return NotFound("Post not found, cannot save post");
        }

        // Fetches the user
        var user = await _userManager.FindByIdAsync(GetUserId());

        // Error handling if the user is not found
        if (user == null)
        {
            _logger.LogError(
                "[PostController] SavePost failed, failed while executing _userManager.FindByIdAsync(GetUserId()) returned null");
            return NotFound("User not found, cannot save post. Please log in again");
        }

        // Checks if the user has already saved the post
        if (user.SavedPosts != null && user.SavedPosts.Any(t => t.PostId == id))
        {
            user.SavedPosts.Remove(post); // Removes the post from the user's saved posts
            await _userManager.UpdateAsync(user); // Updates the user
            return Ok("Post unsaved successfully");
        }

        // If the user has saved posts, create a new list of posts, else add the post to the user's saved posts
        user.SavedPosts ??= new List<Post>();
        user.SavedPosts.Add(post);

        // Updates the user attribute
        await _userManager.UpdateAsync(user);

        return Ok("Post saved successfully");
    }


    // Get request for adding likes to a comment
    [HttpGet("LikeComment/{id:int}")]
    public async Task<IActionResult> NewLikeComment(int id)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        // Fetches comment based on id
        var comment = await _commentRepository.GetTById(id);

        // Error handling if the comment is not found
        if (comment == null)
        {
            _logger.LogError("[PostController] LikeComment failed, failed while executing GetTById() returned null");
            return NotFound("Comment not found, cannot like comment");
        }

        // Fetches the user
        var user = await _userManager.FindByIdAsync(userId);

        // Error handling if the user is not found
        if (user == null)
        {
            _logger.LogError(
                "[PostController] LikeComment failed, failed while executing _userManager.FindByIdAsync(GetUserId()) returned null");
            return NotFound("User not found, cannot like comment. Please log in again");
        }

        // Checks if the user has already liked the comment
        if (user.LikedComments != null && user.LikedComments.Any(t => t.CommentId == id))
        {
            comment.TotalLikes--; // Decrements like on the comment
            user.LikedComments.Remove(comment); // Removes the comment from the user's liked comments
            await _userManager.UpdateAsync(user); // Updates the user
            if (!await _commentRepository.Update(comment)) // Updates the comment
            {
                _logger.LogError("[PostController] LikeComment failed, failed while executing Update()");
                return StatusCode(500, "Internal server error while updating comment please try again");
            }

            // Refreshes the site
            return Ok("Unliked comment successfully");
        }

        // Increments like on the comment
        comment.TotalLikes++;

        // Adds the comment to the user's liked comments
        user.LikedComments ??= new List<Comment>();
        user.LikedComments.Add(comment);

        // Updates the user attribute
        await _userManager.UpdateAsync(user);

        // Updates the comment
        if (!await _commentRepository.Update(comment))
        {
            _logger.LogError("[PostController] LikeComment failed, failed while executing Update()");
            return StatusCode(500, "Internal server error while updating comment please try again");
        }

        return Ok("Liked comment successfully");
    }

    // Get request for adding likes to a comment
    [HttpGet("SaveComment/{id:int}")]
    public async Task<IActionResult> SaveComment(int id)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        // Fetches comment based on id
        var comment = await _commentRepository.GetTById(id);

        // Error handling if the comment is not found
        if (comment == null)
        {
            _logger.LogError("[PostController] SaveComment failed, failed while executing GetTById() returned null");
            return NotFound("Comment not found, cannot save comment");
        }

        // Fetches the user
        var user = await _userManager.FindByIdAsync(GetUserId());

        // Error handling if the user is not found
        if (user == null)
        {
            _logger.LogError(
                "[PostController] SaveComment failed, failed while executing _userManager.FindByIdAsync(GetUserId()) returned null");
            return NotFound("User not found, cannot save comment. Please log in again");
        }

        // Checks if the user has already liked the comment
        if (user.SavedComments != null && user.SavedComments.Any(t => t.CommentId == id))
        {
            user.SavedComments.Remove(comment); // Removes the comment from the user's liked comments
            await _userManager.UpdateAsync(user); // Updates the user
            // Updates the comment
            if (!await _commentRepository.Update(comment))
            {
                _logger.LogError("[PostController] SaveComment failed, failed while executing Update()");
                return StatusCode(500, "Internal server error while updating comment please try again");
            }

            // Refreshes the site
            return Ok("Unsaved comment successfully");
        }

        // Adds the comment to the user's liked comments
        user.SavedComments ??= new List<Comment>();
        user.SavedComments.Add(comment);

        // Updates the user attribute
        await _userManager.UpdateAsync(user);

        // Updates the comment
        if (!await _commentRepository.Update(comment))
        {
            _logger.LogError("[PostController] SaveComment failed, failed while executing Update()");
            return StatusCode(500, "Internal server error while updating comment please try again");
        }

        return Ok("Saved comment successfully");
    }


    [HttpGet("DeleteComment/{id:int}")]
    public async Task<IActionResult> NewDeleteComment(int id)
    {
        var userId = GetUserId();
        if (userId.IsNullOrEmpty()) return StatusCode(403, "User not found, please log in again"); //  403 Forbidden

        // Fetch the comment from database, based on id
        var commentFromDb = await _commentRepository.GetTById(id);

        // Error handling if no comment is found
        if (commentFromDb == null)
        {
            _logger.LogError("[Post controller] DeleteComment failed, GetTById() for tags returned null");
            return NotFound("Comment not found, cannot show post");
        }

        // Checks if the user is the owner of the comment, if not, return to the post
        if (commentFromDb.UserId != GetUserId() && !IsAdmin())
        {
            _logger.LogError("[PostController] DeleteComment failed, user is not the owner of the comment");
            return StatusCode(403, "Could not delete the comment, you are not the owner of the comment");
        }

        // Checks if the model for comments is valid and returns error message, if not, return to the post
        if (!ModelState.IsValid)
            return StatusCode(422, "Could not delete the comment, please try again"); // 422 Unprocessable Entity

        // Checks if the comment has replies, if not, delete the comment
        if (commentFromDb.CommentReplies == null || commentFromDb.CommentReplies.Count == 0)
        {
            if (!await _commentRepository.Delete(commentFromDb.CommentId))
            {
                _logger.LogError("[PostController] DeleteComment failed, failed while executing Delete()");
                return StatusCode(500, "Internal server error while updating comment please try again");
            }
        }
        else
        {
            // Updates the comment in the database
            commentFromDb.DateLastEdited = DateTime.Now;
            commentFromDb.Content = "";
            if (!await _commentRepository.Update(commentFromDb))
            {
                _logger.LogError("[PostController] DeleteComment failed, failed while executing Update()");
                return StatusCode(500, "Internal server error while updating comment please try again");
            }
        }

        // Fetches the post to update the total comments
        var post = await _postRepository.GetTById(commentFromDb.PostId);
        if (post == null)
        {
            _logger.LogError("[PostController] DeleteComment failed, failed while executing GetTById() returned null");
            return NotFound("Post not found, cannot update post");
        }

        post.TotalComments--;
        if (!await _postRepository.Update(post))
        {
            _logger.LogError("[PostController] DeleteComment failed, failed while executing Update()");
            return StatusCode(500, "Internal server error while updating post please try again");
        }

        return Ok("Comment deleted successfully");
    }
}