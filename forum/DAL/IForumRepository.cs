using forum.Models;

namespace forum.DAL;

public interface IForumRepository<TEntity>
{
    Task<IEnumerable<TEntity>?> GetAll();
    Task<TEntity?> GetTById(int id);
    Task<Post?> GetPostById(int id);

    Task<IEnumerable<Post>?> GetAllPosts();
    Task<IEnumerable<Comment>?> GetCommentsByPostId(int postId);
    Task<bool> Create(TEntity entity);
    Task<bool> Update(TEntity entity);
    Task<bool> Delete(int id);


    //https://dotnettutorials.net/lesson/generic-repository-pattern-csharp-mvc/
    //https://learn.microsoft.com/en-us/aspnet/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application#implement-a-generic-repository-and-a-unit-of-work-class
}