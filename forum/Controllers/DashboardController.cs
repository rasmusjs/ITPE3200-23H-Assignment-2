using System.Security.Claims;
using forum.DAL;
using forum.Models;
using forum.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
        var adminDashboardViewModel = new DashboardViewModel
        {
            CategoryList = categories,
            TagList = tags
        };

        return View(adminDashboardViewModel);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(Category category)
    {
        var dbCategory = await _categoryRepository.GetTById(category.CategoryId);

        if (dbCategory == null)
        {
            _logger.LogError($"[Dashboard controller] UpdateCategory() failed, error message: oldCategory is null");
            return NotFound("Category not found");
        }

        // Save the old picture path
        string pictureDeletePath = dbCategory.PicturePath ?? String.Empty;
        string newPicturePath = "";


        Console.WriteLine("Path to delete" + pictureDeletePath);

        // If the user has selected a file
        if (Request.Form.Files.Count > 0) // If the user has selected a file
        {
            newPicturePath = await FileUpload(category);

            if (newPicturePath.IsNullOrEmpty())
            {
                _logger.LogError($"[Dashboard controller] UpdateCategory() failed, error message: fileUpload failed");
                return StatusCode(500, "Could not upload new file");
            }
        }

        if (!ModelState.IsValid)
        {
            _logger.LogError($"[Dashboard controller] UpdateCategory() failed, error message: modelState is invalid");
            return StatusCode(406, "Model state is invalid");
        }

        // Update the category
        dbCategory.Name = category.Name;
        dbCategory.Color = category.Color;

        // Update the picture path if the user has used the text field
        if (!category.PicturePath.IsNullOrEmpty())
        {
            dbCategory.PicturePath = category.PicturePath;
        }


        // If the user has selected a new file, set the new picture path, else keep the old one
        if (!newPicturePath.IsNullOrEmpty())
        {
            dbCategory.PicturePath = newPicturePath;
        }


        // Update the category
        await _categoryRepository.Update(dbCategory);

        // Delete the old picture if it exists
        if (!pictureDeletePath.IsNullOrEmpty())
        {
            if (!DeleteFile(pictureDeletePath))
            {
                _logger.LogError(
                    $"[Dashboard controller] DeleteCategory() failed, error message: deleteFile failed");
                return StatusCode(500, "Internal server error, could not delete file");
            }
        }

        return RedirectToAction("AdminDashboard");
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewCategory(Category category)
    {
        // If the user has selected a file
        if (Request.Form.Files.Count > 0) // If the user has selected a file
        {
            string filePath = await FileUpload(category);

            if (filePath.IsNullOrEmpty())
            {
                _logger.LogError(
                    $"[Dashboard controller] UpdateCategory() failed, error message: fileUpload failed");
                return StatusCode(500, "Could not upload new file");
            }

            // Set the new picture path
            category.PicturePath = filePath;
            //Set the PictureBytes to null
            category.PictureBytes = null;
        }

        if (ModelState.IsValid)
        {
            var result = await _categoryRepository.Create(category);

            if (result == null)
            {
                _logger.LogError($"[Dashboard controller] NewCategory() failed, error message: result is null");
                return NotFound("Category not found");
            }
        }


        return RedirectToAction("AdminDashboard");
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepository.GetTById(id);

        if (category == null)
        {
            _logger.LogError($"[Dashboard controller] DeleteCategory() failed, error message: category is null");
            return NotFound("Category not found");
        }

        // Delete the old picture if it exists
        if (category.PicturePath != null)
        {
            if (!DeleteFile(category.PicturePath))
            {
                _logger.LogError($"[Dashboard controller] DeleteCategory() failed, error message: deleteFile failed");
                return StatusCode(500, "Internal server error, could not delete file");
            }
        }

        var result = await _categoryRepository.Delete(id);
        if (!result)
        {
            _logger.LogError($"[Dashboard controller] DeleteCategory() failed, error message: result is null");
            return NotFound("Category not found");
        }

        return RedirectToAction("AdminDashboard");
    }

    public async Task<string> FileUpload(Category category)
    {
        // Get the file
        var file = Request.Form.Files.FirstOrDefault();

        // If the file is null or empty
        if (file == null || file.Length == 0)
        {
            _logger.LogError($"[Dashboard controller] FileUpload() failed, error message: file is null or empty");
            return "";
        }

        long maxSize = 8 * 1024 * 1024; // 8MB

        if (file.Length > maxSize) // If the file is greater than 8MB
        {
            _logger.LogError($"[Dashboard controller] FileUpload() failed, error message: file to large");
            return "";
        }

        // Create a new file name with a GUID and the file extension
        string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

        // Create the path to the file
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories", fileName);

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

        return "../images/categories/" + fileName;
    }

    public bool DeleteFile(string path)
    {
        string deletePath = path;

        //Replace the ../ with wwwroot/ to get the correct path
        deletePath = deletePath.Replace("../", "wwwroot/");

        Console.WriteLine(deletePath);

        // Check if the file exists, if i does not exist it's probably a external file 
        if (System.IO.File.Exists(deletePath))
        {
            // Delete the file
            System.IO.File.Delete(deletePath);
            //https://learn.microsoft.com/en-us/dotnet/api/system.io.file.delete?view=net-7.0#system-io-file-delete(system-string)

            if (System.IO.File.Exists(deletePath))
            {
                _logger.LogError(
                    $"[Dashboard controller] DeleteCategory() failed, error message: could not delete file");
                return false;
            }
        }

        return true;
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