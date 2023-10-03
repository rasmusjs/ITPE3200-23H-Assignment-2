using Microsoft.EntityFrameworkCore;
using forum.Models;

namespace forum.DAL;

//Source: https://dotnettutorials.net/lesson/generic-repository-pattern-csharp-mvc/
//Source: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application#implement-a-generic-repository-and-a-unit-of-work-class

// A generic repository class for the forum
public class ForumRepository<TEntity> : IForumRepository<TEntity> where TEntity : class
{
    // DB session and logger
    private readonly ForumDbContext _db;
    private readonly ILogger<ForumRepository<TEntity>> _logger;

    // Constructor for initializing the logger
    public ForumRepository(ForumDbContext db, ILogger<ForumRepository<TEntity>> logger)
    {
        _db = db;
        _logger = logger;
    }

    // Fetches all entities from the database
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
            _logger.LogError($"[{typeof(TEntity).Name} Repository] GetAll() failed, error message: {e.Message}");
            return null;
        }
    }

    public async Task<ApplicationUser?> GetUserActivity(string userid)
    {
        // Tries to retrieve all activity from the database as object
        try
        {
            return await _db.Set<ApplicationUser>().Include(user => user.Posts).Include(user => user.Comments)
                .Include(user => user.LikedPosts).Include(user => user.LikedComments)
                .Where(user => user.Id == userid).FirstAsync();
        }
        // Exception error handling if it can't fetch entities from the database
        catch (Exception e)
        {
            _logger.LogError(
                $"[{typeof(TEntity).Name} Repository] GetUserActivity() failed, error message: {e.Message}");
            return null;
        }
    }


    // Get all posts from database, based on a search term 
    public async Task<IEnumerable<Post>?> GetAllPostsByTerm(string term)
    {
        try
        {
            // Make the search term lowercase
            term = term.ToLower();

            // Search in title, content, tags and comments (might be costly)
            var result = await _db.Posts
                .Include(post => post.Tags)
                .Include(post => post.Category)
                .Include(post => post.Comments)
                .Include(post => post.User)
                .Where(post =>
                    ((post.Title.ToLower().Contains(term) || post.Content.ToLower().Contains(term)) ||
                     post.Category!.Name.ToLower().Contains(term) ||
                     post.Tags!.Any(tag => tag.Name!.ToLower().Contains(term)) ||
                     post.Comments!.Any(comment => comment.Content.ToLower().Contains(term)) ||
                     post.User!.UserName.ToLower().Contains(term)
                    )
                )
                .ToListAsync();

            return result;
        }
        // Error handling if it can't fetch posts from the search term
        catch (Exception e)
        {
            _logger.LogError($"[{typeof(TEntity).Name} Repository] GetAll() failed, error message: {e.Message}");
            return null;
        }
    }

    // Fetches posts from the database, based on post id
    public async Task<Post?> GetPostById(int id)
    {
        try
        {
            // Query the database for posts by id. Includes tags and categories (eagerly loading)
            return await _db.Posts.Include(post => post.Tags).Include(post => post.Category)
                .Where(post => post.PostId == id).FirstAsync();
        }
        // Error handling if it can't fetch the post
        catch (Exception e)
        {
            _logger.LogError($"[{typeof(Post).Name} Repository] GetAll() failed, error message: {e.Message}");
            return null;
        }
    }

    // Fetches all posts from the database
    public async Task<IEnumerable<Post>?> GetAllPosts()
    {
        try
        {
            // Query the database for all posts. Includes tags and categories (eagerly loading)
            return await _db.Posts.Include(post => post.Tags).Include(post => post.Category).ToListAsync();
        }
        // Error handling if it can't fetch posts
        catch (Exception e)
        {
            _logger.LogError($"[{typeof(Post).Name} Repository] GetAll() failed, error message: {e.Message}");
            return null;
        }
    }

    // Fetches all posts from the database
    public async Task<IEnumerable<Post>?> GetAllPostActivity(string userId)
    {
        try
        {
            // Query the database for all posts. Includes tags and categories (eagerly loading)
            return await _db.Posts.Include(post => post.Tags).Include(post => post.Category)
                .Where(post => post.UserId.Contains(userId)).ToListAsync();
        }
        // Error handling if it can't fetch posts
        catch (Exception e)
        {
            _logger.LogError($"[{typeof(Post).Name} Repository] GetAll() failed, error message: {e.Message}");
            return null;
        }
    }

    // Fetches comments from the database, based on the comment id
    public async Task<IEnumerable<Comment>?> GetCommentsByPostId(int postId)
    {
        try
        {
            // Query the database for comments by id. Includes tags and categories (eagerly loading)
            return await _db.Comments.Include(comment => comment.CommentReplies)
                .Where(comment => comment.PostId == postId).ToListAsync();
        }
        // Error handling if it can't fetch the comment
        catch (Exception e)
        {
            _logger.LogError($"[ForumRepository] GetCommentsByPostId() failed, error message: {e.Message}");
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
            _logger.LogError(
                "[ForumRepository] entity GetTById(id) failed for TEntityId {TEntityId:0000}, error message: {e}",
                id, e.Message);
            return null;
        }
    }

    // Generic method to fetch any entity based on id
    public async Task<ApplicationUser?> GetUserById(string id)
    {
        try
        {
            // Query the database for all entities with primary key as id
            return await _db.Set<ApplicationUser>().FindAsync(id);
        }
        // Error handling if it can't fetch entities
        catch (Exception e)
        {
            _logger.LogError(
                "[ForumRepository] entity GetUserById(id) failed for TEntityId {TEntityId:0000}, error message: {e}",
                id, e.Message);
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
            return entity;
        }
        // Error handling if it can't create a new entity
        catch (Exception e)
        {
            _logger.LogError("[ForumRepository] entity creation failed for entity {@entity}, error message: {e}",
                entity,
                e.Message);
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
            return true;
        }
        // Error handling if it can't update an entity
        catch (Exception e)
        {
            _logger.LogError(
                "[ForumRepository] entity FindAsync(id) failed when updating the TEntityId {TEntityId:0000}, error message: {e}",
                entity, e.Message);
            return false;
        }
    }

    // Generic method to delete an entity based on id
    public async Task<bool> Delete(int id)
    {
        try
        {
            // Finds the entity in the database
            var entity = await _db.Set<TEntity>().FindAsync(id);
            // Error handling if there are no entity with the provided id
            if (entity == null)
            {
                _logger.LogError("[ForumRepository] entity not found for the TEntityId {TEntityId:0000}", id);
                return false;
            }

            // Removes the entity from the database and save the changes
            _db.Set<TEntity>().Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
        // Error handling if it is not able to delete the entity
        catch (Exception e)
        {
            _logger.LogError("[ForumRepository] deletion failed for the TEntityId {TEntityId:0000}, error message: {e}",
                id, e.Message);
            return false;
        }
    }

    // Method for deleting all the tags from a post based on entity id
    public async Task<bool> RemoveAllPostTags(int id)
    {
        if (id < 0) // id is not valid (negative)
            return false;

        try
        {
            // Source: https://learn.microsoft.com/en-us/ef/core/querying/sql-queries
            // According to the documentation it's faster to do this than to select all the tags and then remove them one by one for updating
            var executeSqlAsync =
                await _db.Database.ExecuteSqlAsync(
                    $"DELETE FROM PostTag WHERE PostsPostId = {id}");


            // Error handling if it could not find the entity 
            if (executeSqlAsync == 0)
            {
                _logger.LogError("[ForumRepository] entity not found for the TEntityId {TEntityId:0000}", id);
                return false;
            }

            return true;
        }
        // Error handling if it failed to delete the tags from the entity
        catch (Exception e)
        {
            _logger.LogError("[ForumRepository] deletion failed for the TEntityId {TEntityId:0000}, error message: {e}",
                id, e.Message);
            return false;
        }
    }
}