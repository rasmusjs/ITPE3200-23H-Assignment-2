using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using forum.Controllers;
using forum.DAL;
using forum.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ForumTest.Controllers;

// Source: https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/testing?view=aspnetcore-6.0
public class PostControllerTest
{
    // Mock models from the PostController
    private readonly Mock<IForumRepository<Post>> _mockPostRepository;
    private readonly Mock<IForumRepository<Tag>> _mockTags;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly Mock<IForumRepository<Category>> _mockCategoryRepository;
    private readonly Mock<IForumRepository<Comment>> _mockCommentRepository;
    private readonly Mock<ForumDbContext> _mockForumDbContext;
    //private readonly Mock<IMemoryCache> _mockMemoryCache;
    private readonly Mock<ILogger<PostController>> _mockLogger;

    private readonly PostController _controller;
    private readonly IMemoryCache _memoryCache;

    // Mock constructor from PostController
    public PostControllerTest()
    {
        // Initialize mocks
        _mockPostRepository = new Mock<IForumRepository<Post>>();
        _mockTags = new Mock<IForumRepository<Tag>>();
        _mockCategoryRepository = new Mock<IForumRepository<Category>>();
        _mockCommentRepository = new Mock<IForumRepository<Comment>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());   // New memory cache to avoid having to mocking it 
        _mockLogger = new Mock<ILogger<PostController>>();

        // https://stackoverflow.com/questions/37630564/how-to-mock-up-dbcontext
        // https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#different-types-of-test-doubles
        _mockForumDbContext = new Mock<ForumDbContext>(new DbContextOptions<ForumDbContext>());

        // Source: https://stackoverflow.com/questions/57291677/creating-instances-for-rolemanager-and-usermanager-for-unit-testing 
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            new Mock<IUserStore<ApplicationUser>>().Object,
            null, null, null, null, null, null, null, null);

        // Mock constructor
        _controller = new PostController(
            _mockCategoryRepository.Object,
            _mockTags.Object,
            _mockPostRepository.Object,
            _mockForumDbContext.Object,
            _mockCommentRepository.Object,
            _mockUserManager.Object,
            _memoryCache,
            _mockLogger.Object
        );
    }

    // GetUserID() HELVETE

    // IsAdmin() HELVETE

    // Method for getting mock posts for testing
    private List<Post> GetMockPosts()
    {
        return new List<Post>
        {
            new Post
            {
                PostId = 1, Title = "Simple ToDo App", Content = "Hello everyone", DateCreated = DateTime.Now.AddDays(-2),
                DateLastEdited = DateTime.Now.AddDays(-2), TotalLikes = 9, UserId = "user1", CategoryId = 1,
                Tags = new List<Tag> { new() { Name = "Beginner" }, new() { Name = "HTML" } }
            },
            new Post
            {
                PostId = 2, Title = "Hello", Content = "Yeah yeah yeah", DateCreated = DateTime.Now.AddDays(-1),
                DateLastEdited = DateTime.Now.AddDays(-1), TotalLikes = 1, UserId = "user2", CategoryId = 2,
                Tags = new List<Tag> { new() { Name = "JavaScript" }, new() { Name = "Java" } }
            },
            new Post
            {
                PostId = 3, Title = "HELP", Content = "I don't understand anything", DateCreated = DateTime.Now,
                DateLastEdited = DateTime.Now, TotalLikes = 11, UserId = "user3", CategoryId = 3,
                Tags = new List<Tag> { new() { Name = "Beginner" }, new() { Name = "Java" } }
            }
        };
    }

    // GetAllPosts(string sortby = "")
   [Fact]
   public async Task GetAllPosts_ReturnPostsOKTest()
   {
       // Arrange
       var mockPosts = GetMockPosts();
       _mockPostRepository.Setup(repo => repo.GetAllPosts(It.IsAny<String>())).ReturnsAsync(mockPosts);
       
       // Act
       var result = await _controller.GetAllPosts("likes");

       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.IsAssignableFrom<IEnumerable<Post>>(okResult.Value);
   }
   
   [Fact]
   public async Task? GetAllPosts_ReturnPostsNotFoundTest()
   {
       // Arrange
       var emptyPosts = new List<Post>();
       _mockPostRepository.Setup(repo => repo.GetAllPosts(It.IsAny<String>())).ReturnsAsync(emptyPosts);
       
       // Act
       var result = await _controller.GetAllPosts("likes");

       // Assert
       var noPostsFound = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal("No posts found", noPostsFound.Value);
   }
   
   // GetPost(int id)
   
   //GetTags()
   
   // GetCategories()
   
   // GetComments(int id)
   
   // Create() Get
   
   //NewCreate(Post post)
   
   // GetPostViewModel()
   
   //NewUpdate(Post post)
   
   // Delete(int id)
   
   // NewDeleteConfirmed(int id)
   
   // NewCreateComment(Comment comment)
   
   // NewUpdateComment(Comment comment)
   
   // NewLikePost(int id)
   
   // NewLikeComment(int id)
   
   // NewDeleteComment(int id)
   
}