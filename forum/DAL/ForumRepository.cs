using Microsoft.EntityFrameworkCore;
using forum.Models;

namespace forum.DAL;

//https://dotnettutorials.net/lesson/generic-repository-pattern-csharp-mvc/
//https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application#implement-a-generic-repository-and-a-unit-of-work-class

public class ForumRepository<TEntity> : IForumRepository<TEntity> where TEntity : class
{
    private readonly ForumDbContext _db;
    private readonly ILogger<ForumRepository<TEntity>> _logger;

    public ForumRepository(ForumDbContext db, ILogger<ForumRepository<TEntity>> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IEnumerable<TEntity>?> GetAll()
    {
        try
        {
            return await _db.Set<TEntity>().ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"[{typeof(TEntity).Name} Repository] GetAll() failed, error message: {e.Message}");
            return null;
        }
    }


    public async Task<IEnumerable<Post>?> GetAllPosts()
    {
        try
        {
            return await _db.Posts.Include(post => post.Tags).Include(post => post.Category).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"[{typeof(Post).Name} Repository] GetAll() failed, error message: {e.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<Comment>?> GetCommentsByPostId(int postId)
    {
        try
        {
            return await _db.Comments.Include(comment => comment.CommentReplies)
                .Where(comment => comment.PostId == postId)
                .Where(comment => comment.ParentCommentId == null).ToListAsync();
        }
        catch (Exception e)
        {
            _logger.LogError($"[{typeof(Comment).Name} Repository] GetAll() failed, error message: {e.Message}");
            return null;
        }
    }

    public async Task<TEntity?> GetTById(int id)
    {
        try
        {
            return await _db.Set<TEntity>().FindAsync(id);
        }
        catch (Exception e)
        {
            _logger.LogError(
                "[ForumRepository] entity FindAsync(id) failed when GetTEntityById for TEntityId {TEntityId:0000}, error message: {e}",
                id, e.Message);
            return null;
        }
    }

    public async Task<bool> Create(TEntity entity)
    {
        try
        {
            _db.Set<TEntity>().Add(entity);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[ForumRepository] entity creation failed for entity {@entity}, error message: {e}",
                entity,
                e.Message);
            return false;
        }
    }

    public async Task<bool> Update(TEntity entity)
    {
        try
        {
            _db.Set<TEntity>().Update(entity);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(
                "[ForumRepository] entity FindAsync(id) failed when updating the TEntityId {TEntityId:0000}, error message: {e}",
                entity, e.Message);
            return false;
        }
    }

    public async Task<bool> Delete(int id)
    {
        try
        {
            var entity = await _db.Set<TEntity>().FindAsync(id);
            if (entity == null)
            {
                _logger.LogError("[ForumRepository] entity not found for the TEntityId {TEntityId:0000}", id);
                return false;
            }

            _db.Set<TEntity>().Remove(entity);
            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError("[ForumRepository] deletion failed for the TEntityId {TEntityId:0000}, error message: {e}",
                id, e.Message);
            return false;
        }
    }
}