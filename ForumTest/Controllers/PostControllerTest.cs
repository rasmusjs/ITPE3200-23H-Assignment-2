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
    
    // Method for fetching a mock user without a claim
    private ApplicationUser GetMockUser()
    {
        return new ApplicationUser
        {
            Id = "user1", UserName = "user1", Email = "mail@mail.com", CreationDate = DateTime.Now
        };
    }

    // Method for creating a mock user with a claim to mock logged in users
    private (ApplicationUser, ClaimsPrincipal) CreateMockUser()
    {
        var userId = "user1";
        var mockUser = new ApplicationUser
        {
            Id = userId,
            UserName = "user1",
            Email = "mail@mail.com",
            CreationDate = DateTime.Now
        };
        
        // Mock user claim for user1
        var userClaims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, mockUser.UserName)
        };
        var claimsIdentity = new ClaimsIdentity(userClaims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return (mockUser, claimsPrincipal);
    }

    
    // GetUserID() HELVETE

    // IsAdmin() HELVETE
    
    
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
                User = new ClaimsPrincipal(new ClaimsIdentity(new[]
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
   
   // Method for testing Create when user is not logged in
   [Fact]
   public async Task Create_NotLoggedInTest()
   {
       // Act
       var result = await _controller.NewCreate(GetMockPosts().First());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value); 
   }
   
   // Method for testing Create function when it returns OK
   [Fact]
   public async Task Create_ReturnPostOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
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
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       
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
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockCategory = GetMockCategories().First();
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
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       
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
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockTag = GetMockTags().First();
       
       var mockPost = GetMockPosts().First(p => p.PostId == 3); // Use the post with null category from the mock posts
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync(mockPost);
       _mockCategoryRepository.Setup(repo => repo.GetTById(mockPost.CategoryId)).ReturnsAsync((Category) null!);
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
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       var mockPost = GetMockPosts().First(); // Use the first post (valid post) from the mock posts
       
       // Simulate failure to create post:
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync((Post) null!);
       
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
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockTag = GetMockTags().First();
       var mockCategory = GetMockCategories().First();
       var mockPost = GetMockPosts().First(); // Use the first post (valid post) from the mock posts
       
       _mockPostRepository.Setup(repo => repo.Create(It.IsAny<Post>())).ReturnsAsync(mockPost);
       // Simulate failure to find user:
       _mockUserManager.Setup(repo => repo.FindByIdAsync(It.IsAny<string>()))!.ReturnsAsync((ApplicationUser) null!);
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
   [Fact]
   public async Task Create_UserUpdateSucessTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id
       
       _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(mockUser); // Ensure FindByIdAsync returns the mock user
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success); 
       
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
       _mockUserManager.Verify(m => m.UpdateAsync(It.Is<ApplicationUser>(u => u.Id == userId)), Times.Once); // Verify UpdateAsync was called
       // Verify the response
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal(mockPost.PostId, okResult.Value);
   }
   
   // Method for testing Update when user is not logged in
   [Fact]
   public async Task Update_NotLoggedInTest()
   {
       // Act
       var result = await _controller.NewUpdate(GetMockPosts().First());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value); 
   }
   
   // Method for testing Update function when it returns OK
   // This does not work because of this line in the controller: _forumDbContext.Entry(postFromDb).State = EntityState.Detached;
   //[Fact]
   public async Task Update_ReturnPostOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

        // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the post to user
       var mockPost = GetMockPosts().First(p => p.UserId == userId);
       mockPost.User = mockUser;
       mockPost.UserId = userId;
       mockUser.Posts = GetMockPosts();
       
       // Mock the repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(true);
       _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(mockUser); // Ensure FindByIdAsync returns the mock user
       
       // Act
       var result = await _controller.NewUpdate(mockPost);
       
       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal(mockPost.PostId, okResult.Value);
   }

   // Method for testing Update function when it returns Internal Server Error because of invalid Model State
   [Fact]
   public async Task Update_ReturnModelStateInvalidTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockPost = GetMockPosts().First();
       
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(true); 
       
       // Mocking failed Model State
       // Source: https://stackoverflow.com/questions/17346866/model-state-validation-in-unit-tests
       _controller.ModelState.AddModelError("Tags", "Post not valid, cannot create post");
       
       // Act
       var result = await _controller.NewUpdate(mockPost);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode); 
   }

   // Method to test Update function when it returns Internal Server Error because of null post
   [Fact]
   public async Task Update_ReturnPostNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var emptyPost = new Post { PostId = 1 }; // Post that don't exist
       
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync((Post) null!); // Return null post
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(true);  
       
       // Act
       var result = await _controller.NewUpdate(emptyPost);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating post please try again", statusCodeResult.Value);
   }

   // Method to test Update function when it returns invalid user 
   [Fact]
   public async Task Update_InvalidUserIdTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockPost = GetMockPosts().First();
       mockPost.UserId = "forbidden"; // Set invalid user id
       
       // Mock the repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(true);
       _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(mockUser); // Ensure FindByIdAsync returns the mock user
       
       // Act
       var result = await _controller.NewUpdate(mockPost);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("You are not the owner of the post",statusCodeResult.Value);
   }

   // Method for testing Update function when there are null tags
   // This does not work because of this line in the controller: _forumDbContext.Entry(postFromDb).State = EntityState.Detached;
   //[Fact]
   public async Task Update_NullTagsTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the post to user
       var mockPost = GetMockPosts().First(p => p.UserId == userId);
       mockPost.User = mockUser;
       mockPost.UserId = userId;
       mockUser.Posts = GetMockPosts();
       
       // Mock the repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(true); 
       _mockTags.Setup(repo => repo.GetAll()).ReturnsAsync((List<Tag>) null!); // Return null tags

       // Act
       var result = await _controller.NewUpdate(mockPost);

       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("Tags not found, cannot update post", statusCodeResult.Value);
   }

   // Method for testing Update function when there is an update failure
   // This does not work because of this line in the controller: _forumDbContext.Entry(postFromDb).State = EntityState.Detached;
   //[Fact]
   public async Task Update_ReturnUpdateFailureTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the post to user
       var mockPost = GetMockPosts().First(p => p.UserId == userId);
       mockPost.User = mockUser;
       mockPost.UserId = userId;
       mockUser.Posts = GetMockPosts();
       var mockTags = GetMockTags();
       
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockTags.Setup(repo => repo.GetAll()).ReturnsAsync(mockTags);
       _mockUserManager.Setup(m => m.FindByIdAsync(userId)).ReturnsAsync(mockUser); // Ensure FindByIdAsync returns the mock user
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(false); 

       // Act
       var result = await _controller.NewUpdate(mockPost);

       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating post please try again", statusCodeResult.Value);
   }
   
   // Method for testing DeleteConfirmed when user is not logged in
   [Fact]
   public async Task DeleteConfirmed_NotLoggedInTest()
   {
       // Act
       var result = await _controller.NewDeleteConfirmed(It.IsAny<int>());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value); 
   }
   
   // Method for testing DeleteConfirmed function when it returns OK
   [Fact]
   public async Task DeleteConfirmed_ReturnPostOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the post to user
       var mockPost = GetMockPosts().First(p => p.UserId == userId);
       mockPost.User = mockUser;
       mockPost.UserId = userId;
       mockUser.Posts = GetMockPosts();
       
       // Mock repos 
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockPostRepository.Setup(repo => repo.Delete(It.IsAny<int>())).ReturnsAsync(true);
       
       // Act
       var result = await _controller.NewDeleteConfirmed(mockPost.PostId);
       
       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Post deleted successfully", okResult.Value); 
   }

   // Method for testing DeleteConfirmed function when it returns post not found
   [Fact]
   public async Task DeleteConfirmed_ReturnPostNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var emptyPost = new Post { PostId = 1 }; // Post that don't exist
       
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync((Post) null!); // Return null post
       
       // Act
       var result = await _controller.NewDeleteConfirmed(emptyPost.PostId); 

       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("Post not found, cannot delete post", statusCodeResult.Value); 
   }

   // Method for testing DeleteConfirmed function when it returns invalid user
   [Fact]
   public async Task DeleteConfirmed_ReturnUserNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockPost = GetMockPosts().First();
       mockPost.UserId = "forbidden"; // Set user id to another user
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost); 
       
       // Act
       var result = await _controller.NewUpdate(mockPost);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("You are not the owner of the post",statusCodeResult.Value); 
   }

   // Method for testing DeleteConfirmed function when it returns delete failure
   [Fact]
   public async Task DeleteConfirmed_ReturnDeleteFailureTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the post to user
       var mockPost = GetMockPosts().First(p => p.UserId == userId);
       mockPost.User = mockUser;
       mockPost.UserId = userId;
       mockUser.Posts = GetMockPosts();
       
       // Mock repos 
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockPostRepository.Setup(repo => repo.Delete(It.IsAny<int>())).ReturnsAsync(false); // Mock delete failure
       
       // Act
       var result = await _controller.NewDeleteConfirmed(mockPost.PostId); 
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while deleting post please try again", statusCodeResult.Value);
   }
   
   // Method for testing CreateComment function when it returns OK
   [Fact]
   public async Task CreateComment_ReturnCommentOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the comment to user
       mockUser.Posts = GetMockPosts();
       var mockComment = GetMockComments().First(p => p.UserId == userId);
       mockComment.User = mockUser;
       mockComment.UserId = userId;
       mockUser.Comments = GetMockComments();
       
       var mockPost = GetMockPosts().First();
       mockComment.Post = mockPost;

       // Mock repos
       _mockCommentRepository.Setup(repo => repo.Create(It.IsAny<Comment>())).ReturnsAsync(mockComment);
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment.Post);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(true);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);

       // Act
       var result = await _controller.NewCreateComment(mockComment); 

       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Created comment successfully", okResult.Value);  
   }

   // Method for testing CreateComment function when it returns invalid Model State
   [Fact]
   public async Task CreateComment_ReturnInvalidModelStateTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockComment = GetMockComments().First();
       
       // Mocking failed Model State
       // Source: https://stackoverflow.com/questions/17346866/model-state-validation-in-unit-tests
       _controller.ModelState.AddModelError("Invalid", "Could not create the comment, the comment is not valid");
       
       // Act
       var result = await _controller.NewCreateComment(mockComment);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(422, statusCodeResult.StatusCode);
       Assert.Equal("Could not create the comment, the comment is not valid", statusCodeResult.Value);
   }

   // Method for testing CreateComment when user is not logged in
   [Fact]
   public async Task CreateComment_NotLoggedInTest()
   {
       // Act
       var result = await _controller.NewCreateComment(GetMockComments().First());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value); 
   }
   
   // Method for testing CreateComment when the returning new comment is null
   [Fact]
   public async Task CreateComment_ReturnErrorCreatingCommentTest()
   {
      // Arrange
      var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
      var userId = mockUser.Id; // Set user id

      // Set the User property of the controller to the test user
      _controller.ControllerContext = new ControllerContext
      {
          HttpContext = new DefaultHttpContext { User = claimsPrincipal }
      };
       
      var mockComment = GetMockComments().First(p => p.UserId == userId);
      _mockCommentRepository.Setup(repo => repo.Create(It.IsAny<Comment>())).ReturnsAsync((Comment) null!);

      // Act
      var result = await _controller.NewCreateComment(mockComment); 
      
      // Assert
      var statusCodeResult = Assert.IsType<ObjectResult>(result);
      Assert.Equal(500, statusCodeResult.StatusCode);
      Assert.Equal("Internal server error while creating comment please try again", statusCodeResult.Value);
   }
   
   // Method for testing CreateComment when the post for the comment is null and not found 
   [Fact]
   public async Task CreateComment_PostNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the comment to user
       mockUser.Posts = GetMockPosts();
       var mockComment = GetMockComments().First(p => p.UserId == userId);
       mockComment.User = mockUser;
       mockComment.UserId = userId;
       mockUser.Comments = GetMockComments();
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.Create(It.IsAny<Comment>())).ReturnsAsync(mockComment);
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync((Post) null!);
       
       // Act
       var result = await _controller.NewCreateComment(mockComment);
       
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("Post not found, cannot like post", statusCodeResult.Value);
   }
   
   // Method for testing CreateComment when it fails to update the post with the comment
   [Fact]
   public async Task CreateComment_FailedToUpdateTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the comment to user
       mockUser.Posts = GetMockPosts();
       var mockComment = GetMockComments().First(p => p.UserId == userId);
       mockComment.User = mockUser;
       mockComment.UserId = userId;
       mockUser.Comments = GetMockComments();
       
       var mockPost = GetMockPosts().First();
       mockComment.Post = mockPost;
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.Create(It.IsAny<Comment>())).ReturnsAsync(mockComment);
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment.Post);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(false);

       // Act
       var result = await _controller.NewCreateComment(mockComment); 

       // Assert 
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating post please try again", statusCodeResult.Value); 
   }
   
   // Method for testing UpdateComment when it returns OK 
   [Fact]
   public async Task UpdateComment_ReturnCommentOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the comment to user
       mockUser.Posts = GetMockPosts();
       var mockComment = GetMockComments().First(p => p.UserId == userId);
       mockComment.User = mockUser;
       mockComment.UserId = userId;
       mockUser.Comments = GetMockComments();
       
       var mockPost = GetMockPosts().First();
       mockComment.Post = mockPost;

       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(true);

       // Act
       var result = await _controller.NewUpdateComment(mockComment); 

       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Comment updated successfully", okResult.Value);   
   }

   // Method for testing UpdateComment when user is not logged in
   [Fact]
   public async Task UpdateComment_ReturnNotLoggedInTest()
   {
       // Act
       var result = await _controller.NewCreateComment(GetMockComments().First());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value);  
   }

   // Method for testing UpdateComment when model state is invalid
   [Fact]
   public async Task UpdateComment_ReturnInvalidModelStateTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockComment = GetMockComments().First();
       
       // Mocking failed Model State
       // Source: https://stackoverflow.com/questions/17346866/model-state-validation-in-unit-tests
       _controller.ModelState.AddModelError("Invalid", "Could not update the comment, the comment is not valid");
       
       // Act
       var result = await _controller.NewUpdateComment(mockComment);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(422, statusCodeResult.StatusCode);
       Assert.Equal("Could not update the comment, the comment is not valid", statusCodeResult.Value); 
   }

   // Method for testing UpdateComment when comment is not found
   [Fact]
   public async Task UpdateComment_ReturnCommentNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockComment = GetMockComments().First(p => p.UserId == userId);
       
       // Mock repo
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync((Comment) null!);
       
       // Act
       var result = await _controller.NewUpdateComment(mockComment);
       
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("Comment not found, cannot update comment", statusCodeResult.Value); 
   }

   // Method for testing UpdateComment when user is not the owner of the comment
   [Fact]
   public async Task UpdateComment_ReturnNotCommentOwnerTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the comment to user
       mockUser.Posts = GetMockPosts();
       var mockComment = GetMockComments().First(p => p.UserId == userId);
       mockComment.User = mockUser;
       mockComment.UserId = "123456789"; // Set userId to another user
       mockUser.Comments = GetMockComments();
       
       var mockPost = GetMockPosts().First();
       mockComment.Post = mockPost;
       
       // Mock repo
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment);
       
       // Act
       var result = await _controller.NewUpdateComment(mockComment);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("You are not the owner of the comment", statusCodeResult.Value);  
   }

   // Method for testing UpdateComment when the comment is not updated
   [Fact]
   public async Task UpdateComment_ReturnErrorUpdatingCommentTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim
       var userId = mockUser.Id; // Set user id

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Explicitly set the comment to user
       mockUser.Posts = GetMockPosts();
       var mockComment = GetMockComments().First(p => p.UserId == userId);
       mockComment.User = mockUser;
       mockComment.UserId = mockUser.Id;
       mockUser.Comments = GetMockComments();
       
       var mockPost = GetMockPosts().First();
       mockComment.Post = mockPost;
       
       // Mock repo
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(false); // Simulate update failure
       
       // Act
       var result = await _controller.NewUpdateComment(mockComment);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating comment please try again", statusCodeResult.Value);   
   }
   
   // Method for testing LikePost when user has liked a post and returns unlike OK
   [Fact]
   public async Task LikePost_ReturnUnlikeOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Fetch a post that the user has liked 
       var mockPost = GetMockPosts().First();
       mockUser.LikedPosts = GetMockPosts(); // User has liked all posts

       // Mock repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(true);

       // Act
       var result = await _controller.NewLikePost(mockPost.PostId); 

       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Post unliked successfully", okResult.Value);  
   }

   // Method for testing LikePost when user has not liked a post and returns like OK
   [Fact]
   public async Task LikePost_ReturnLikeOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Fetch a post that the user has not liked 
       var mockPost = GetMockPosts().First();

       // Mock repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(true);

       // Act
       var result = await _controller.NewLikePost(mockPost.PostId); 

       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Post liked successfully", okResult.Value); 
   }

   // Method for testing LikePost when user is not logged in
   [Fact]
   public async Task LikePost_ReturnUserNotLoggedInTest()
   {
       // Act
       var result = await _controller.NewLikePost(It.IsAny<int>());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value);   
   }

   [Fact]
   public async Task LikePost_ReturnPostNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       }; 
       
       // Mock repo to return null post
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync((Post) null!);
       
       // Act
       var result = await _controller.NewLikePost(It.IsAny<int>()); // Pass in any post id
       
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("Post not found, cannot like post", statusCodeResult.Value); 
   }

   // Method for testing LikePost when user is not found
   [Fact]
   public async Task LikePost_ReturnUserNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Fetch a post 
       var mockPost = GetMockPosts().First();

       // Mock repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>()))!.ReturnsAsync((ApplicationUser) null!); // Return null user

       // Act
       var result = await _controller.NewLikePost(mockPost.PostId);
       
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("User not found, cannot like post. Please log in again", statusCodeResult.Value);  
   }

   // Method for testing LikePost when it fails to update post when unliking
   [Fact]
   public async Task LikePost_ReturnUpdateUnlikeFailureTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Fetch a post that the user has liked 
       var mockPost = GetMockPosts().First();
       mockUser.LikedPosts = GetMockPosts(); // User has liked all posts

       // Mock repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(false); // Simulate update failure

       // Act
       var result = await _controller.NewLikePost(mockPost.PostId);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating post please try again", statusCodeResult.Value);  
   }

   // Method for testing LikePost when it fails to update post when liking
   [Fact]
   public async Task LikePost_ReturnUpdateLikeFailureTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       // Fetch a post that the user has not liked 
       var mockPost = GetMockPosts().First();

       // Mock repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockPostRepository.Setup(repo => repo.Update(It.IsAny<Post>())).ReturnsAsync(false);

       // Act
       var result = await _controller.NewLikePost(mockPost.PostId);
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating post please try again", statusCodeResult.Value);   
   }
   
   // LikePost fail to update user attribute??
   
   // Method  for testing SavePost when it returns save OK 
   [Fact]
   public async Task SavePost_ReturnSavePostOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockPost = GetMockPosts().First();
       
       // Mock repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost); 
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       
         // Act
         var result = await _controller.SavePost(mockPost.PostId); 
         
         // Assert
         var okResult = Assert.IsType<OkObjectResult>(result);
         Assert.Equal("Post saved successfully", okResult.Value);  
   }
   
   // Method  for testing SavePost when it returns unsave OK 
   [Fact]
   public async Task SavePost_ReturnUnsavePostOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockPost = GetMockPosts().First();
       mockUser.SavedPosts = GetMockPosts(); // Save all posts
       
       // Mock repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost); 
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       
       // Act
       var result = await _controller.SavePost(mockPost.PostId); 
         
       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Post unsaved successfully", okResult.Value);  
   }
   
   // Method for testing SavePost when user is not logged in
   [Fact]
   public async Task SavePost_ReturnUserNotLoggedInTest()
   {
       // Act
       var result = await _controller.SavePost(It.IsAny<int>());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value);   
   }

   // Method for testing SavePost when post is not found
   [Fact]
   public async Task SavePost_ReturnPostNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };   
       
       // Mock repo returning null posts
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync((Post) null!); 
       
       // Act
       var result = await _controller.SavePost(It.IsAny<int>()); 
       
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("Post not found, cannot save post", statusCodeResult.Value);  
   }

   // Method for testing SavePost when user is not found
   [Fact]
   public async Task SavePost_ReturnUserNotFound()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockPost = GetMockPosts().First();
       
       // Mock repos
       _mockPostRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockPost); 
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>()))!.ReturnsAsync((ApplicationUser) null!); // Return null user
       
       // Act
       var result = await _controller.SavePost(mockPost.PostId); 
       
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("User not found, cannot save post. Please log in again", statusCodeResult.Value); 
   }
   
   // SavePost failed update test
   
   // Method for testing LikeComment when it returns like OK 
   [Fact]
   public async Task LikeComment_ReturnLikeOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockComment = GetMockComments().First();
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(true);

       // Act
       var result = await _controller.NewLikeComment(mockComment.CommentId); 

       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Liked comment successfully", okResult.Value);  
   }
   
   // Method for testing LikeComment when it returns unlike OK 
   [Fact]
   public async Task LikeComment_ReturnUnlikeOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockComment = GetMockComments().First();
       mockUser.LikedComments = GetMockComments(); // User has liked all comments
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(true);

       // Act
       var result = await _controller.NewLikeComment(mockComment.CommentId); 

       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Unliked comment successfully", okResult.Value);  
   }

   // Method for testing LikeComment when user is not logged in
   [Fact]
   public async Task LikeComment_ReturnUserNotLoggedInTest()
   {
       // Act
       var result = await _controller.NewLikeComment(It.IsAny<int>());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value);    
   } 
   
   // Method for testing LikeComment when comment is not found
   [Fact]
   public async Task LikeComment_ReturnCommentNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };

       var mockComment = GetMockComments().First(); 

       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync((Comment) null!);

       // Act
       var result = await _controller.NewLikeComment(mockComment.CommentId); // Pass in any comment id
       
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("Comment not found, cannot like comment", statusCodeResult.Value);   
   }
   
   // Method for testing LikeComment when user is not found
   [Fact]
   public async Task LikeComment_ReturnUserNotFound()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockComment = GetMockComments().First();
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>()))!.ReturnsAsync((ApplicationUser) null!); // Return null user

       // Act
       var result = await _controller.NewLikeComment(mockComment.CommentId);  
       
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("User not found, cannot like comment. Please log in again", statusCodeResult.Value);   
   }

   //Method for testing LikeComment when it fails to update comment when liking
   [Fact]
   public async Task LikeComment_ReturnUpdateLikeFailTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockComment = GetMockComments().First();
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(false);

       // Act
       var result = await _controller.NewLikeComment(mockComment.CommentId); 

       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating comment please try again", statusCodeResult.Value);   
   }
   
   //Method for testing LikeComment when it fails to update comment when liking
   [Fact]
   public async Task LikeComment_ReturnUpdateUnlikeFailTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };
       
       var mockComment = GetMockComments().First();
       mockUser.LikedComments = GetMockComments(); // User has liked all comments
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment);
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(false);

       // Act
       var result = await _controller.NewLikeComment(mockComment.CommentId); 

       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating comment please try again", statusCodeResult.Value);   
   }
   
   // Method  for testing SaveComment when it returns save OK 
   [Fact]
   public async Task SaveComment_ReturnSaveCommentOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockComment = GetMockComments().First();
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment); 
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(true);
       
       // Act
       var result = await _controller.SaveComment(mockComment.PostId); 
         
       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Saved comment successfully", okResult.Value);  
   } 
   
   // Method  for testing SaveComment when it returns save OK 
   [Fact]
   public async Task SaveComment_ReturnUnsaveCommentOkTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockComment = GetMockComments().First();
       mockUser.SavedComments = GetMockComments(); // Save all comments
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment); 
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(true);
       
       // Act
       var result = await _controller.SaveComment(mockComment.PostId); 
         
       // Assert
       var okResult = Assert.IsType<OkObjectResult>(result);
       Assert.Equal("Unsaved comment successfully", okResult.Value);  
   } 
   
   // Method for testing SaveComment when user is not logged in
   [Fact]
   public async Task SaveComment_ReturnUserNotLoggedInTest()
   {
       // Act
       var result = await _controller.SaveComment(It.IsAny<int>());
       
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(403, statusCodeResult.StatusCode);
       Assert.Equal("User not found, please log in again", statusCodeResult.Value);    
   }  
   
   // Method for testing SaveComment when comment is not found
   [Fact]
   public async Task SaveComment_ReturnCommentNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockComment = GetMockComments().First();
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync((Comment) null!); 
       
       // Act
       var result = await _controller.SaveComment(mockComment.PostId); 
         
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("Comment not found, cannot save comment", statusCodeResult.Value);
   }
   
   // Method  for testing SaveComment when it returns user not found
   [Fact]
   public async Task SaveComment_ReturnUserNotFoundTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockComment = GetMockComments().First();
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment); 
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync((ApplicationUser) null!);
       
       // Act
       var result = await _controller.SaveComment(mockComment.PostId); 
         
       // Assert
       var statusCodeResult = Assert.IsType<NotFoundObjectResult>(result);
       Assert.Equal(404, statusCodeResult.StatusCode);
       Assert.Equal("User not found, cannot save comment. Please log in again", statusCodeResult.Value);
   }  
   
   // Method  for testing SaveComment when it returns update failed for like
   [Fact]
   public async Task SaveComment_ReturnUpdateLikeFailedTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockComment = GetMockComments().First();
       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment); 
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(false);
       
       // Act
       var result = await _controller.SaveComment(mockComment.PostId); 
         
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating comment please try again", statusCodeResult.Value);
   }  
   
   // Method  for testing SaveComment when it returns update failed for unlike
   [Fact]
   public async Task SaveComment_ReturnUpdateUnlikeFailedTest()
   {
       // Arrange
       var (mockUser, claimsPrincipal) = CreateMockUser(); // Create user with claim

       // Set the User property of the controller to the test user
       _controller.ControllerContext = new ControllerContext
       {
           HttpContext = new DefaultHttpContext { User = claimsPrincipal }
       };  
       
       var mockComment = GetMockComments().First();
       mockUser.SavedComments = GetMockComments(); // Save all comments

       
       // Mock repos
       _mockCommentRepository.Setup(repo => repo.GetTById(It.IsAny<int>())).ReturnsAsync(mockComment); 
       _mockUserManager.Setup(m => m.FindByIdAsync(It.IsAny<String>())).ReturnsAsync(mockUser);
       _mockUserManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
       _mockCommentRepository.Setup(repo => repo.Update(It.IsAny<Comment>())).ReturnsAsync(false);
       
       // Act
       var result = await _controller.SaveComment(mockComment.PostId); 
         
       // Assert
       var statusCodeResult = Assert.IsType<ObjectResult>(result);
       Assert.Equal(500, statusCodeResult.StatusCode);
       Assert.Equal("Internal server error while updating comment please try again", statusCodeResult.Value);
   }  
   
   // NewDeleteComment(int id)
   
}