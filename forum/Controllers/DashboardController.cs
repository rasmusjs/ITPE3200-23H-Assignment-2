using System.Security.Claims;
using forum.DAL;
using forum.Models;
using forum.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace forum.Controllers;

public class DashBoardController : Controller
{
    private readonly ILogger<ApplicationUser> _logger; // Ikke satt opp enda!

    // Connect the controller to the different models
    private readonly IForumRepository<ApplicationUser> _userRepository;
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly IForumRepository<Tag> _tagsRepository;


    // Constructor for Dependency Injection to the Data Access Layer from the different repositories
    public DashBoardController(
        IForumRepository<ApplicationUser> userRepository, IForumRepository<Category> categoryRepository
        , IForumRepository<Tag> tags,
        ILogger<ApplicationUser> logger)
    {
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _tagsRepository = tags;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public string GetUserId()
    {
        //https://stackoverflow.com/questions/29485285/can-not-find-user-identity-getuserid-method
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }


    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        // Get all activity for the user
        var userActivity = await _userRepository.GetUserActivity(GetUserId());

        // If no posts, return NotFound
        if (userActivity == null)
        {
            _logger.LogError($"[Dashboard controller] Dashboard() failed, error message: userActivity is null");
            return NotFound("Post list not found");
        }

        return View(userActivity);
    }


    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        // Fetching the categories and tags data
        IEnumerable<Category>? categories = await _categoryRepository.GetAll();
        IEnumerable<Tag>? tags = await _tagsRepository.GetAll();

        // Exception if there are no tags or categories to show the user
        if (categories == null || tags == null)
            throw new InvalidOperationException("Categories or tags not found, cannot create post");

        // New view model for creating a post
        var adminDashboardViewModel = new AdminDashboardViewModel
        {
            CategoryList = categories,
            TagSelectList = tags
        };

        return View(adminDashboardViewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(Category category)
    {
        if (ModelState.IsValid)
        {
            await _categoryRepository.Update(category);
        }

        return RedirectToAction("AdminDashboard");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewCategory(Category category)
    {
        if (ModelState.IsValid)
        {
            // If the user has selected a file
            if (Request.Form.Files.Count > 0) // If the user has selected a file
            {
                // Get the file
                var file = Request.Form.Files.FirstOrDefault();

                // If the file is null or empty
                if (file == null || file.Length == 0)
                {
                    return RedirectToPage("");
                }

                long maxSize = 8 * 1024 * 1024; // 8MB

                if (file.Length > maxSize) // If the file is greater than 8MB
                {
                    return RedirectToPage("");
                }

                // Create a new file name with a GUID and the file extension
                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

                // Create the path to the file
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories",
                    fileName);

                // If the directory does not exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                // Copy the file to the path
                await using (FileStream fs = System.IO.File.Create(filePath))
                {
                    await file.CopyToAsync(fs);
                }

                //category.PicturePath = filePath;

                category.PicturePath = "../images/categories/" + fileName;

                Console.WriteLine("File uploaded successfully");
            }

            Console.WriteLine("Picture path " + category.PicturePath);

            //Set the PictureBytes to null, since we don't want to save the image in the database
            category.PictureBytes = null;

            var result = await _categoryRepository.Create(category);

            if (result == null)
            {
                _logger.LogError($"[Dashboard controller] NewCategory() failed, error message: result is null");
                return NotFound("Category not found");
            }

            return RedirectToAction("AdminDashboard");
        }


        return RedirectToAction("AdminDashboard");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryRepository.Delete(id);
        if (!result)
        {
            _logger.LogError($"[Dashboard controller] DeleteCategory() failed, error message: result is null");
            return NotFound("Category not found");
        }

        return RedirectToAction("AdminDashboard");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTag(Tag tag)
    {
        if (ModelState.IsValid)
        {
            await _tagsRepository.Update(tag);
        }

        return RedirectToAction("AdminDashboard");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewTag(Category tag)
    {
        if (ModelState.IsValid)
        {
            await _categoryRepository.Create(tag);
        }

        return RedirectToAction("AdminDashboard");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        var result = await _tagsRepository.Delete(id);
        if (!result)
        {
            _logger.LogError($"[Dashboard controller] DeleteCategory() failed, error message: result is null");
            return NotFound("Category not found");
        }

        return RedirectToAction("AdminDashboard");
    }
}