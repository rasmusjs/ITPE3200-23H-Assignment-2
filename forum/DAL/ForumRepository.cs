using forum.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace forum.DAL;

//Source: https://dotnettutorials.net/lesson/generic-repository-pattern-csharp-mvc/
//Source: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application#implement-a-generic-repository-and-a-unit-of-work-class

// A generic repository class for the forum
public class ForumRepository<TEntity> : IForumRepository<TEntity> where TEntity : class
{
    // Initialize DB session and logger
    private readonly ForumDbContext _db;
    private readonly ILogger<ForumRepository<TEntity>> _logger;

    // Constructor for initializing the logger and DB
    public ForumRepository(ForumDbContext db,
        ILogger<ForumRepository<TEntity>> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Function for fetching all entities from the database
    public async Task<IEnumerable<TEntity>?> GetAll()
    {
        // Tries to retrieve all records from the database as a list
        try
        {
            return await _db.Set<TEntity>().ToListAsync();
        }
        // Exception error handling if it can't fetch entities from the database
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("GetAll", e);
            return null;
        }
    }

    // Function for fetching all entities from the database
    public async Task<IEnumerable<Comment>?> GetAllCommentsByPostId(int id)
    {
        // Tries to retrieve all records from the database as a list
        try
        {
            var comments = await _db.Set<Comment>().Where(comment => comment.PostId == id)
                .ToListAsync();

            return comments;
        }
        // Exception error handling if it can't fetch entities from the database
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("GetAll", e);
            return null;
        }
    }

    // Function to fetch user activity from the database
    public async Task<ApplicationUser?> GetUserActivity(string userId)
    {
        // Checks if the user id is null or empty
        if (userId.IsNullOrEmpty()) return null;

        // Tries to retrieve all activity from the database as object
        try
        {
            return await _db.Set<ApplicationUser>().Include(user => user.Posts).Include(user => user.Comments)
                .Include(user => user.LikedPosts).Include(user => user.LikedComments)
                .Where(user => user.Id == userId).FirstAsync();
        }
        // Exception error handling if it can't fetch entities from the database
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("GetUserActivity", e);
            return null;
        }
    }

    // Get all posts from database, based on a search term 
    public async Task<IEnumerable<Post>?> GetAllPostsByTerm(string term, string userId = "")
    {
        try
        {
            // Make the search term lowercase
            term = term.ToLower();

            // Search in title, content, tags and comments (might be costly)
            var posts = await _db.Posts
                .Include(post => post.Tags)
                .Include(post => post.Category)
                .Include(post => post.Comments)
                .Include(post => post.User)
                .Where(post =>
                    post.Title.ToLower().Contains(term) || post.Content.ToLower().Contains(term) ||
                    post.Category!.Name.ToLower().Contains(term) ||
                    post.Tags!.Any(tag => tag.Name.ToLower().Contains(term)) ||
                    post.Comments!.Any(comment => comment.Content.ToLower().Contains(term)) ||
                    post.User!.UserName.ToLower().Contains(term)
                )
                .ToListAsync();

            if (!posts.Any())
            {
                _logger.LogInformation("[Forum Repository] GetAllPostsByTerm() found no posts");
                return null;
            }


            // If user is logged in, add likes to posts
            if (userId != "") posts = await AddLikeToPosts(posts, userId);

            return posts;
        }
        // Error handling if it can't fetch posts from the search term
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("GetAllPostsByTerm", e);
            return null;
        }
    }

    // Fetches posts from the database, based on post id
    public async Task<Post?> GetPostById(int id, string userId = "")
    {
        try
        {
            // Query the database for posts by id. Includes tags and categories (eagerly loading)
            var post = await _db.Posts
                .Include(post => post.Tags)
                .Include(post => post.Category)
                .Include(post => post.Comments)
                .Include(post => post.User)
                .Where(post => post.PostId == id)
                .FirstAsync();

            // If there is a userId, get user data
            if (userId != "")
            {
                // Fetches the user activity
                var user = await GetUserActivity(userId);

                // If there is a user
                if (user != null)
                {
                    if (user.LikedPosts != null)
                    {
                        // Checks if the user has liked the post
                        if (user.LikedPosts.Any(t => t.PostId == id))
                            post.IsLiked = true;
                    }

                    // If the user have liked comments add the likes to them
                    if (user.LikedComments != null && post.Comments != null)
                    {
                        post.Comments = await AddLikeToComments(post.Comments, userId);
                    }
                }
            }

            return post;
        }
        // Error handling if it can't fetch the post from db
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("GetPostById", e);
            return null;
        }
    }

    // Fetches all posts from the database
    public async Task<IEnumerable<Post>?> GetAllPosts(string userId = "")
    {
        try
        {
            // Query the database for all posts. Includes tags and categories (eagerly loading)
            var posts = await _db.Posts.Include(post => post.Tags).Include(post => post.Category)
                .Include(post => post.User!).ToListAsync();

            if (!posts.Any())
            {
                _logger.LogInformation("[Forum Repository] GetAllPosts() found no posts");
                return null;
            }

            // If user is logged in, add likes to posts
            if (userId != "") posts = await AddLikeToPosts(posts, userId);

            return posts;
        }
        // Error handling if it can't fetch posts from db
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("GetAllPosts", e);
            return null;
        }
    }


    // Generic method to fetch any entity based on id
    public async Task<TEntity?> GetTById(int id)
    {
        try
        {
            // Query the database for all entities with primary key as id
            return await _db.Set<TEntity>().FindAsync(id);
        }
        // Error handling if it can't fetch entities
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("GetTById", e);
            return null;
        }
    }


    // Generic method to create and save an entity
    public async Task<TEntity?> Create(TEntity entity)
    {
        try
        {
            // Tries to add an entity, save the changes and return the entity
            _db.Set<TEntity>().Add(entity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("[Forum Repository] Entity added to the database: {entity}", entity);
            return entity;
        }
        // Error handling if it can't create a new entity in the db
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("Create", e);
            return null;
        }
    }

    // Generic method to update an entity
    public async Task<bool> Update(TEntity entity)
    {
        try
        {
            // Tries to update an entity in the database and save the changes
            _db.Set<TEntity>().Update(entity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("[Forum Repository] Entity updated in the database: {entity}", entity);
            return true;
        }
        // Error handling if it can't update an entity in the db
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("Update", e);
            return false;
        }
    }

    // Generic method to delete an entity based on id
    public async Task<bool> Delete(int id)
    {
        if (id < 0) // id is not valid (negative)
        {
            LogError($"Delete, entity not found for id {id}", new Exception("Entity not found"));
            return false;
        }

        try
        {
            // Finds the entity in the database
            var entity = await _db.Set<TEntity>().FindAsync(id);
            // Error handling if there are no entity with the given id 
            if (entity == null)
            {
                // Sends function name and error to the LogError function
                LogError("Delete", new Exception("Entity not found"));
                return false;
            }

            // Removes the entity from the database and save the changes
            _db.Set<TEntity>().Remove(entity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("[Forum Repository] Entity removed from the database: {entity}", entity);
            return true;
        }
        // Error handling if it is not able to delete the entity
        catch (Exception e)
        {
            LogError("Delete", e);
            return false;
        }
    }

    // Method for deleting all the tags from a post based on entity id
    public async Task<bool> RemoveAllPostTags(int id)
    {
        if (id < 0) // id is not valid (negative)
        {
            // Sends function name and error to the LogError function
            LogError("RemoveAllPostTags", new Exception($"Entity not found for id {id}"));
            return false;
        }

        try
        {
            // Source: https://learn.microsoft.com/en-us/ef/core/querying/sql-queries
            // According to the documentation it's faster to do this than to select all the tags and then remove them one by one for updating
            var executeSqlAsync =
                await _db.Database.ExecuteSqlAsync($"DELETE FROM PostTag WHERE PostsPostId = {id}");

            // Error handling if it could not find the entity 
            if (executeSqlAsync == 0)
            {
                // Sends function name and error to the LogError function
                LogError("RemoveAllPostTags", new Exception($"Entity not found for id {id}"));
                return false;
            }

            return true;
        }
        // Error handling if it failed to delete the tags from the entity
        catch (Exception e)
        {
            // Sends function name and error to the LogError function
            LogError("RemoveAllPostTags", e);
            return false;
        }
    }


    private async Task<List<Post>?> AddLikeToPosts(List<Post>? posts, string userId)
    {
        // Check if posts are null or empty
        if (posts == null || !posts.Any())
        {
            _logger.LogWarning(
                "[Forum Repository] AddLikeToPosts() could not add Likes to posts, warning: Posts are null or empty");
            return posts;
        }

        try
        {
            // Fetches the user activity
            var user = await GetUserActivity(userId);

            if (user is { LikedPosts: not null })
            {
                // Loops through all posts
                foreach (var post in posts)
                    // Checks if the user has liked the post
                    if (user.LikedPosts.Any(t => t.PostId == post.PostId))
                    {
                        post.IsLiked = true;
                    }
            }
            else
            {
                _logger.LogInformation("[Forum Repository] AddLikeToPosts() user have not liked any posts");
            }
        }
        catch (Exception e)
        {
            LogError("AddLikeToPosts", e);
        }

        return posts;
    }

    private async Task<List<Comment>?> AddLikeToComments(List<Comment>? comments, string userId)
    {
        // Check if comments are null or empty
        if (comments == null || !comments.Any())
        {
            return comments;
        }

        try
        {
            // Fetches the user activity
            var user = await GetUserActivity(userId);

            if (user is { LikedComments: not null })
            {
                // Loops through all posts
                foreach (var comment in comments)
                    // Checks if the user has liked the comment
                    if (user.LikedComments.Any(t => t.CommentId == comment.CommentId))
                        comment.IsLiked = true;
            }
            else
            {
                LogError("AddLikeToComments", new Exception("GetUserActivity() returned null"));
            }
        }
        catch (Exception e)
        {
            LogError("AddLikeToComments", e);
        }

        return comments;
    }

// Common method for logging errors
    private void LogError(string methodName, Exception exception)
    {
        _logger.LogError(
            $"[{typeof(TEntity).Name} Repository] {methodName}() failed, error message: {exception.Message}");
    }
}