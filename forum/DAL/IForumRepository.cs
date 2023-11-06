using forum.Models;

namespace forum.DAL;

public interface IForumRepository<TEntity>
{
    // Get all entities
    Task<IEnumerable<TEntity>?> GetAll();

    // Get entity by id
    Task<TEntity?> GetTById(int id);

    // Get post by id
    Task<Post?> GetPostById(int id, string userId = "");

    // Get all posts, with optional user id
    Task<IEnumerable<Post>?> GetAllPosts(string userId = "");

    // Get all comments
    Task<IEnumerable<Comment>?> GetAllCommentsByPostId(int id);


    // Get all posts by search term
    Task<IEnumerable<Post>?> GetAllPostsByTerm(string term, string userId = "");
    Task<ApplicationUser?> GetUserActivity(string userId);

    // Create entity
    Task<TEntity?> Create(TEntity entity);

    // Update entity
    Task<bool> Update(TEntity entity);

    // Delete by id (generic)
    Task<bool> Delete(int id);

    // Remove all tags on post
    Task<bool> RemoveAllPostTags(int id);

    // Creating generic methods:
    //Source: https://dotnettutorials.net/lesson/generic-repository-pattern-csharp-mvc/
    //Source: https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application#implement-a-generic-repository-and-a-unit-of-work-class
}