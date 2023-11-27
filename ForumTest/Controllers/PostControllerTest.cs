using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using forum.Controllers;
using forum.DAL;
using forum.Models;
using Microsoft.AspNetCore.Http;
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
        _memoryCache = new MemoryCache(new MemoryCacheOptions()); // New memory cache to avoid having to mocking it 
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
                PostId = 1, Title = "Simple ToDo App", Content = "Hello everyone",
                DateCreated = DateTime.Now.AddDays(-2),
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

    // Method for testing GetAllPosts function when it returns OK 
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

    // Method for testing GetAllPosts function when it returns NotFound
    [Fact]
    public async Task? GetAllPosts_ReturnPostsNotFoundTest()
    {
        // Arrange
        var emptyPosts = new List<Post>(); // Empty list of posts
        _mockPostRepository.Setup(repo => repo.GetAllPosts(It.IsAny<String>())).ReturnsAsync(emptyPosts);

        // Act
        var result = await _controller.GetAllPosts("likes");

        // Assert
        var noPostsFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No posts found", noPostsFound.Value);
    }

    // Method for testing GetPost function when it returns OK
    [Fact]
    public async Task GetPost_ReturnPostOkTest()
    {
        // Arrange
        var mockPosts = GetMockPosts();
        int postId = 1;
        var userId = "user1";
        _mockPostRepository.Setup(repo => repo.GetPostById(postId, It.IsAny<string>()))
            .ReturnsAsync(mockPosts.FirstOrDefault(p => p.PostId == postId));

        // Mock the user id with a claim
        // Source: https://weblogs.asp.net/ricardoperes/unit-testing-the-httpcontext-in-controllers
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    { new Claim(ClaimTypes.NameIdentifier, userId) }))
            }
        };

        // Act
        var result = await _controller.GetPost(postId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPost = Assert.IsType<Post>(okResult.Value);
        Assert.Equal(postId, returnedPost.PostId);
    }

    // Method for testing GetPost function when it returns NotFound
    [Fact]
    public async Task GetPost_ReturnPostNotFoundTest()
    {
        // Arrange
        var emptyPost = new List<Post>(); // Empty post list
        _mockPostRepository.Setup(repo => repo.GetPostById(It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(emptyPost.FirstOrDefault(p => p.PostId == 1));

        // Act
        var result = await _controller.GetPost(1);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Post not found, cannot show post", notFoundResult.Value);
    }
    
    // Method for getting mock tags for testing
    private List<Tag> GetMockTags()
    {
        return new List<Tag>
        {
            new Tag {TagId = 1, Name = "Beginner"},
            new Tag {TagId = 2, Name = "HTML"},
            new Tag {TagId = 3, Name = "JavaScript"},
            new Tag {TagId = 4, Name = "Java"}
        };
    }

    // Method for testing GetTags function when it returns OK and count amount of tags
    [Fact]
    public async Task GetTags_ReturnTagsOkTest()
    {
        // Arrange
        var mockTags = GetMockTags();
        _mockTags.Setup(repo => repo.GetAll()).ReturnsAsync(mockTags);

        // Act
        var result = await _controller.GetTags();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedTags = Assert.IsType<List<Tag>>(okResult.Value);
        Assert.Equal(mockTags.Count, returnedTags.Count);
    }
    
    // Method for testing GetTags function when it returns NotFound
    [Fact]
    public async Task GetTags_ReturnNotFoundTest()
    {
        // Arrange
        var emptyTags = new List<Tag>(); // Empty tag list
        _mockTags.Setup(repo => repo.GetAll()).ReturnsAsync(emptyTags);

        // Act
        var result = await _controller.GetTags();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Tags not found", notFoundResult.Value);
    }

    // Method for getting mock categories for testing
    private List<Category> GetMockCategories()
    {
        return new List<Category>
        {
            new Category {CategoryId = 1, Name = "General"},
            new Category {CategoryId = 2, Name = "Science"},
            new Category {CategoryId = 3, Name = "Back End"}
        };
    }
    
    // Method for testing GetCategories function when it returns OK and count amount of categories
    [Fact]
    public async Task GetCategiories_ReturnCategoriesOkTest()
    {
        // Arrange
        var mockCategories = GetMockCategories();
        _mockCategoryRepository.Setup(repo => repo.GetAll()).ReturnsAsync(mockCategories);

        // Act
        var result = await _controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategories = Assert.IsType<List<Category>>(okResult.Value);
        Assert.Equal(mockCategories.Count, returnedCategories.Count);
    }

    // Method for testing GetCategories function when it returns NotFound
    [Fact]
    public async Task GetCategories_ReturnNotFoundTest()
    {
        // Arrange
        var emptyCategories = new List<Category>(); // Empty category list
        _mockCategoryRepository.Setup(repo => repo.GetAll()).ReturnsAsync(emptyCategories);
        
        // Act
        var result = await _controller.GetCategories();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Categories not found", notFoundResult.Value);
    }

    private List<Comment> GetMockComments()
    {
        return new List<Comment>
        {
            new Comment
            {
                CommentId = 1, Content = "Hello everyone", DateCreated = DateTime.Now.AddDays(-2), PostId = 1,
                UserId = "user1"
            },
            new Comment
            {
                CommentId = 2, Content = "Yeah yeah yeah", DateCreated = DateTime.Now.AddDays(-1), PostId = 2,
                UserId = "user2"
            },
            new Comment
            {
                CommentId = 3, Content = "I don't understand anything", DateCreated = DateTime.Now, PostId = 3,
                UserId = "user3"
            }
        };  
    }
    
   // Method for testing GetComments function when it returns OK and count amount of comments
   [Fact]
   public async Task GetComments_ReturnCommentsOKTest()
   {
       // Arrange
       int commentId = 1;
       var mockComments = GetMockComments();
       // Mock the repository to retrieve all comments by comment id to list
       _mockCommentRepository.Setup(repo => repo.GetAllCommentsByPostId(commentId)).ReturnsAsync(mockComments.Where(c => c.CommentId == commentId).ToList());
       
       // Act
       var result = await _controller.GetComments(commentId);
       
       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result); 
       var returnedComments = Assert.IsType<List<Comment>>(okResult.Value); 
       // Check that the count is equal to the amount of comments with the same comment id
       Assert.Equal(mockComments.Count(c => c.CommentId == commentId), returnedComments.Count); 
       // Check that the comments are equal to the mock comments
       Assert.All(returnedComments, comments => Assert.Contains(comments, mockComments));
   }

   // Method for testing GetComments function when it returns NotFound
   [Fact]
   public async Task GetComments_ReturnNotFoundTest()
   {
       // Arrange
       int commentId = 1;
       var emptyComments = new List<Comment>(); // Empty comment list
       _mockCommentRepository.Setup(repo => repo.GetAllCommentsByPostId(commentId)).ReturnsAsync(emptyComments.Where(c => c.CommentId == commentId).ToList());

       // Act
       var result = await _controller.GetComments(commentId);

       // Assert
       var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal("Comments not found", notFoundResult.Value);
   }
   
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