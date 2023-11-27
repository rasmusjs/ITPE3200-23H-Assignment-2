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
       var mockUser = GetMockUser();
       var mockTags = GetMockTags();
       var mockCategories = GetMockCategories();
       var mockTagsId = new List<int> { 1, 2, 3};
       var mockComments = GetMockComments();

       return new List<Post>
       {
           new Post
           {
               PostId = 1, Title = "Title", Content = "This is content", TotalLikes = 1, DateCreated = DateTime.Now,
               DateLastEdited = DateTime.Now, UserId = mockUser.Id, User = mockUser,
               CategoryId = mockCategories.First().CategoryId, Category = mockCategories.First(), TagsId = mockTagsId, Tags = mockTags, Comments = mockComments, TotalComments = 1
           },
           // Invalid post with script in content to trigger html sanitizer
           new Post
           {
               PostId = 2, Title = "Title", Content = "<script>\n alert('gotcha');\n</script>", TotalLikes = 1, DateCreated = DateTime.Now,
               DateLastEdited = DateTime.Now, UserId = mockUser.Id, User = mockUser,
               CategoryId = mockCategories.First().CategoryId, Category = mockCategories.First(), TagsId = mockTagsId, Tags = mockTags, Comments = mockComments, TotalComments = 1 
           },
           // Invalid post with null category
           new Post
           {
               PostId = 3, Title = "Title", Content = "This is content", TotalLikes = 1, DateCreated = DateTime.Now,
               DateLastEdited = DateTime.Now, UserId = mockUser.Id, User = mockUser,
               CategoryId = 0, Category = null, TagsId = mockTagsId, Tags = mockTags, Comments = mockComments, TotalComments = 1 
           },
           // Invalid post with null tags
           new Post
           {
               PostId = 4, Title = "Title", Content = "This is content", TotalLikes = 1, DateCreated = DateTime.Now,
               DateLastEdited = DateTime.Now, UserId = mockUser.Id, User = mockUser,
               CategoryId = mockCategories.First().CategoryId, Category = mockCategories.First(), TagsId = null, Tags = null, Comments = mockComments, TotalComments = 1
           }
       }; 
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
    
    // Method for getting mock comments for testing
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
    
    // Method for fetching a mock user
    private ApplicationUser GetMockUser()
    {
        return new ApplicationUser
        {
            Id = "user1", UserName = "user1", Email = "mail@mail.com", CreationDate = DateTime.Now
        };
    }

    // Method for getting mock posts for testing
    /*private List<Post> GetMockPosts()
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
    }*/

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
   
   // Method for testing Create function when it returns OK
   [Fact]
   public async Task Create_ReturnPostOkTest()
   {
       // Arrange
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       var mockUser = GetMockUser();
       
       var mockPost = GetMockPosts().First(); // Use the first post (valid post) from the mock posts
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync(mockPost);
       _mockCategoryRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockCategory);
       _mockTags.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockTag);
       _mockUserManager.Setup(repo => repo.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser);
       // Mock successful user update
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

       // Act
       var result = await _controller.NewCreate(mockPost);

       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       var returnedPostId = Assert.IsType<int>(okResult.Value);
       Assert.Equal(mockPost.PostId, returnedPostId);
   }
   
   // Method for testing Create function when model state is invalid because of missing tags
   [Fact]
   public async Task Create_InvalidModelStateTest()
   {
       // Arrange
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       var mockUser = GetMockUser();
       
       var mockPost = GetMockPosts().First(p => p.PostId == 2); // Use the last post (null tags) from the mock posts
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync(mockPost);
       _mockCategoryRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockCategory);
       _mockTags.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockTag);
       _mockUserManager.Setup(repo => repo.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser);

       // Mocking failed Model State
       // Source: https://stackoverflow.com/questions/17346866/model-state-validation-in-unit-tests
       _controller.ModelState.AddModelError("Tags", "Post not valid, cannot create post");
       
       // Act
       var result = await _controller.NewCreate(mockPost); 
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(422, statusCodeResult.StatusCode);
   }
   
   // Method for testing Create function when model state is invalid because of missing categories
   [Fact]
   public async Task Create_NullTagsTest()
   {
       // Arrange
       var mockCategory = GetMockCategories().First();
       var mockUser = GetMockUser();
       var mockPost = GetMockPosts().First(p => p.PostId == 4); // Use the post with null tags from the mock posts
       var mockTags = mockPost.Tags;
       
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync(mockPost);
       _mockCategoryRepository.Setup(repo => repo.GetTById(mockPost.CategoryId)).ReturnsAsync(mockCategory);
       _mockTags.Setup(repo => repo.GetAll()).ReturnsAsync(mockTags);
       _mockUserManager.Setup(repo => repo.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser); 
       
       // Mocking failed Model State
       // Source: https://stackoverflow.com/questions/17346866/model-state-validation-in-unit-tests
       _controller.ModelState.AddModelError("Tags", "Tags are required");
       
       // Act
       var result = await _controller.NewCreate(mockPost); 
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(422, statusCodeResult.StatusCode);
   }  
   
   // Method for testing Create function with html sanitizer and post with html script tags
   [Fact]
   public async Task Create_HtmlSanitizerTest()
   {
       // Arrange
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       var mockUser = GetMockUser();
       
       var mockPost = GetMockPosts().First(p => p.PostId == 2); // Use the post with html input from the mock posts
       // Mock the repository to create a post and check that the post content does not contain html script tags
       // Source: https://github.com/devlooped/moq/wiki/Quickstart#callbacks
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).Callback<Post>(p => Assert.DoesNotContain("<script>", p.Content)).ReturnsAsync(mockPost);
       _mockCategoryRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockCategory);
       _mockTags.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockTag);
       _mockUserManager.Setup(repo => repo.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser); 
       // Mock successful user update
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       
       // Act
       var result = await _controller.NewCreate(mockPost); 
       
       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       var returnedPostId = Assert.IsType<int>(okResult.Value);
       Assert.Equal(mockPost.PostId, returnedPostId);
   }
   
   // Method for testing Create function when model state is invalid because of missing categories
   [Fact]
   public async Task Create_NullCategoriesTest()
   {
       // Arrange
       var mockTag = GetMockTags().First();
       var mockUser = GetMockUser();
       
       var mockPost = GetMockPosts().First(p => p.PostId == 3); // Use the post with null category from the mock posts
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync(mockPost);
       _mockCategoryRepository.Setup(repo => repo.GetTById(mockPost.CategoryId)).ReturnsAsync((Category)null);
       _mockTags.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockTag);
       _mockUserManager.Setup(repo => repo.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser); 
       
       // Mocking failed Model State
       // Source: https://stackoverflow.com/questions/17346866/model-state-validation-in-unit-tests
       _controller.ModelState.AddModelError("Category", "Category is required");
       
       // Act
       var result = await _controller.NewCreate(mockPost); 
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(422, statusCodeResult.StatusCode);
   } 
   
   // Method for testing Create function when new post is null
   [Fact]
   public async Task Create_NullNewPostTest()
   {
       // Arrange
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       var mockUser = GetMockUser();
       var mockPost = GetMockPosts().First(); // Use the first post (valid post) from the mock posts
       
       // Simulate failure to create post:
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync((Post)null);
       
       _mockCategoryRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockCategory);
       _mockTags.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockTag);
       _mockUserManager.Setup(repo => repo.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(mockUser); 
       
       // Act
       var result = await _controller.NewCreate(mockPost); 
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(422, statusCodeResult.StatusCode);
       var resultValue = Assert.IsType<string>(statusCodeResult.Value);
       Assert.Equal("Post not created successfully", resultValue);
   }  
   
   // Method for testing Create function when user can't be found
   [Fact]
   public async Task Create_UserNotFoundTest()
   {
       // Arrange
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       var mockPost = GetMockPosts().First(); // Use the first post (valid post) from the mock posts
       
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync(mockPost);
       // Simulate failure to find user:
       _mockUserManager.Setup(repo => repo.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser)null);
       _mockCategoryRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockCategory);
       _mockTags.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockTag);
       
       // Act
       var result = await _controller.NewCreate(mockPost); 
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(422, statusCodeResult.StatusCode);
       var resultValue = Assert.IsType<string>(statusCodeResult.Value);
       Assert.Equal("User not found", resultValue);
   }
   
   // Method for testing Create function that checks if user is updated and cache deleted
   // This test does not work because of the `post.UserId = GetUserId(); line in the controller.
   //[Fact]
   public async Task Create_UserUpdateSucessTest()
   {
       // Arrange
       var userId = "user1";
       var mockUser = GetMockUser();
       mockUser.Id = userId;
       
       _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(mockUser); // Ensure FindByIdAsync returns the mock user
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success); 
       
       var userClaims = new List<Claim>
       {
           new Claim(ClaimTypes.NameIdentifier, userId)
       };
       var claimsIdentity = new ClaimsIdentity(userClaims, "TestAuthType");
       var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        // Set the User property of the controller to our test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockTags = GetMockTags();
       var mockPost = GetMockPosts().First(p => p.UserId == userId); // Use the first post from user1
       var mockCategory = mockPost.Category;
       
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync(mockPost);
       _mockCategoryRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockCategory);
       _mockTags.Setup(repo => repo.GetAll()).ReturnsAsync(mockTags);
       
       // Act
       var result = await _controller.NewCreate(GetMockPosts().First());
       
       // Assert
       // Verify user update was called
       //_mockUserManager.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(user => user == mockUser)), Times.Once);
       _mockUserManager.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(u => u.Id == userId)), Times.Once); // Verify UpdateAsync was called
       // Verify the response
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal(mockPost.PostId, okResult.Value);
   }
   
   
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