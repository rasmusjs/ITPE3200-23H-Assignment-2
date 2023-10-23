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
    private readonly IForumRepository<Category> _categoryRepository;
    private readonly ILogger<ApplicationUser> _logger; // Kommentert ut i program.cs
    private readonly IForumRepository<Tag> _tagsRepository;

    // Connect the controller to the different models
    private readonly IForumRepository<ApplicationUser> _userRepository;

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

    // Get request to fetch user identity
    [HttpGet]
    [Authorize]
    public string GetUserId()
    {
        //https://stackoverflow.com/questions/29485285/can-not-find-user-identity-getuserid-method
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    // Get request to fetch the Dashboard view
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        // Initialize variable
        ApplicationUser? userActivity;

        try
        {
            // Try to get all activity for the user
            userActivity = await _userRepository.GetUserActivity(GetUserId());
        }
        catch (Exception e)
        {
            // Exception and error logging if the server fail to fetch data
            _logger.LogError("[Dashboard controller] an error occured when trying to fetch user activity: {e}", e);
            return StatusCode(500, "Internal server error. Please try again later.");
        }

        // If no posts, return NotFound and log error
        if (userActivity == null)
        {
            _logger.LogError("[Dashboard controller] Dashboard() failed, error message: userActivity is null");
            return NotFound("User data not found");
        }

        return View(userActivity);
    }

    // Method for fetching the admin dashboard view
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDashboard()
    {
        // Initialize categories and tags
        IEnumerable<Category>? categories;
        IEnumerable<Tag>? tags;

        try
        {
            // Fetching the categories and tags data
            categories = await _categoryRepository.GetAll();
            tags = await _tagsRepository.GetAll();
        }
        catch (Exception e)
        {
            // Exception and error logging if the server can't fetch the data
            _logger.LogError("[Dashboard controller] An exception occurred while fetching categories or tags: {e}",
                e);
            return StatusCode(500, "Internal server error. Please try again later.");
        }

        // Exception and error logging if there are no tags or categories to show the user
        if (categories == null || tags == null)
        {
            _logger.LogError(
                "[Dashboard controller] _categoryRepository.GetAll() and/or _tagsRepository.GetAll() returned null");
            return NotFound("Categories or tags not found, cannot create post");
        }

        // New view model for creating a post
        var adminDashboardViewModel = new DashboardViewModel
        {
            CategoryList = categories,
            TagList = tags
        };

        return View(adminDashboardViewModel);
    }

    // Method for updating category (with pictures)
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateCategory(Category category)
    {
        // Initialize the variable
        Category? dbCategory;

        try
        {
            // Try to get the category from the database
            dbCategory = await _categoryRepository.GetTById(category.CategoryId);
        }
        catch (Exception e)
        {
            // Exception and error logging if the server can't fetch the data
            _logger.LogError(
                "[Dashboard controller] An exception occurred while fetching category from the database: {e}",
                e);
            return StatusCode(500, "Internal server error. Please try again later.");
        }

        // Check if there are categories and return error message and log it
        if (dbCategory == null)
        {
            _logger.LogError("[Dashboard controller] UpdateCategory() failed, error message: oldCategory is null");
            return NotFound("Category not found");
        }

        // Save the old picture path
        var pictureDeletePath = dbCategory.PicturePath ?? string.Empty;
        var newPicturePath = "";

        // Sets file as the first file uploaded in the http request form
        var file = Request.Form.Files.FirstOrDefault();

        // If the user has selected a file
        if (file != null)
        {
            // Tries to upload file
            _logger.LogInformation("[Dashboard controller] Attempting to upload a file.");
            newPicturePath = await FileUpload(file);

            // If the picture path is null or empty, throw error and log it
            if (newPicturePath.IsNullOrEmpty())
            {
                _logger.LogError("[Dashboard controller] UpdateCategory() failed, error message: fileUpload failed");
                return StatusCode(500, "Could not upload new file");
            }

            // Log success of file upload
            _logger.LogInformation("[Dashboard controller] File upload succeeded. New path: {newPicturePath}",
                newPicturePath);
        }

        // Checks if the submitted form values passes validation. Throws error message and log it if not
        if (!ModelState.IsValid)
        {
            _logger.LogError("[Dashboard controller] UpdateCategory() failed, error message: modelState is invalid");
            return StatusCode(406, "Model state is invalid");
        }

        // Update the category
        _logger.LogInformation("[Dashboard controller] Attempting to update the category.");
        dbCategory.Name = category.Name;
        dbCategory.Color = category.Color;


        // Update the picture path if the user has used the text field
        if (!category.PicturePath.IsNullOrEmpty()) dbCategory.PicturePath = category.PicturePath;

        // If the user has selected a new file, set the new picture path, else keep the old one
        if (!newPicturePath.IsNullOrEmpty()) dbCategory.PicturePath = newPicturePath;

        try
        {
            // Update the category
            await _categoryRepository.Update(dbCategory);
            _logger.LogInformation("[Dashboard controller] Category update was successful.");
        }
        catch (Exception e)
        {
            // Exception and error logging if the server can't fetch the data 
            _logger.LogError("[Dashboard controller] An exception occurred while updating the category: {e}", e);
            return StatusCode(500, "Internal server error. Please try again later.");
        }

        // Delete the old picture if it exists
        if (!newPicturePath.IsNullOrEmpty())
        {
            _logger.LogInformation("[Dashboard controller] Attempting to delete old picture.");

            if (!DeleteFile(pictureDeletePath))
            {
                _logger.LogError(
                    "[Dashboard controller] DeleteCategory() failed, error message: deleteFile failed");
                return StatusCode(500, "Internal server error, could not delete file");
            }

            _logger.LogInformation("[Dashboard controller] Old picture deleted.");
        }

        return RedirectToAction("AdminDashboard");
    }

    // Function for creating new category
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewCategory(Category category)
    {
        // Sets file as the first file uploaded in the http request form 
        var file = Request.Form.Files.FirstOrDefault();

        // If the user has selected a file
        if (file != null)
        {
            // Sets filepath
            var filePath = await FileUpload(file);
            _logger.LogInformation("[Dashboard controller] Attempting to upload a file.");

            // Checks if the filepath is null/empty and throws error and logs it
            if (filePath.IsNullOrEmpty())
            {
                _logger.LogError(
                    $"[Dashboard controller] UpdateCategory() failed, error message: fileUpload failed, File: {file.FileName}");
                return StatusCode(500, "Could not upload new file");
            }

            // Set the new picture path
            category.PicturePath = filePath;
            //Set the PictureBytes to null
            category.PictureBytes = null;
        }

        // Checks if the submitted form values passes validation. Throws error message and log it if not
        if (ModelState.IsValid)
        {
            try
            {
                // Try to create new category
                var createCategory = await _categoryRepository.Create(category);

                // Error handling if createCategory is null
                if (createCategory == null)
                {
                    _logger.LogError("[Dashboard controller] NewCategory() failed, error message: result is null");
                    return NotFound("Category not found");
                }

                _logger.LogInformation("[Dashboard controller] Successfully created new category.");
            }
            catch (Exception e)
            {
                // Throws exception and logs it if the server can't create new category
                _logger.LogError($"[Dashboard controller] NewCategory() exception occurred: {e}");
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }
        else
        {
            // Throws error and logs it if model state is not valid
            _logger.LogError("[Dashboard controller] NewCategory() failed, error message: modelState is invalid");
            return BadRequest("Model state is invalid");
        }

        return RedirectToAction("AdminDashboard");
    }

    // Function for deleting a category
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        // Initialize the variable
        Category? category;

        try
        {
            // Get the category from the database
            category = await _categoryRepository.GetTById(id);
        }
        catch (Exception e)
        {
            // Throws exception and logs it if the server can't fetch the category 
            _logger.LogError("[Dashboard controller] An exception occurred while fetching category for deletion: {e}",
                e);
            return StatusCode(500, "Internal server error. Please try again later.");
        }

        // If the category does not exist, return not found
        if (category == null)
        {
            _logger.LogError("[Dashboard controller] DeleteCategory() failed, error message: category is null");
            return NotFound("Category not found");
        }

        try
        {
            // If the picture path is not null, delete the file. If the file is in the wwwroot folder, delete it
            // DeleteFile will return true if the file does not exist
            if (category.PicturePath != null && !DeleteFile(category.PicturePath))
            {
                _logger.LogError("[Dashboard controller] DeleteCategory() failed, error message: deleteFile failed");
                return StatusCode(500, "Internal server error, could not delete file");
            }
        }
        catch (Exception e)
        {
            // Exception and logging if DeleteFile() does not work
            _logger.LogError("[Dashboard controller] An exception occurred while deleting file: {e}", e);
            return StatusCode(500, "Internal server error, could not delete file");
        }

        // Initialize variable
        bool deleteCategory;

        try
        {
            // Try to delete the category
            deleteCategory = await _categoryRepository.Delete(id);
        }
        catch (Exception e)
        {
            // If Delete() fails, throw exception and log the error
            _logger.LogError("[Dashboard controller] An exception occurred while deleting category: {e}", e);
            return StatusCode(500, "Internal server error. Please try again later.");
        }

        // If it could not delete the category, throw error and log it
        if (!deleteCategory)
        {
            _logger.LogError("[Dashboard controller] DeleteCategory() failed, error message: result is null");
            return NotFound("Category not found");
        }

        return RedirectToAction("AdminDashboard");
    }

    // Function for uploading file
    public async Task<string> FileUpload(IFormFile file)
    {
        // Set the max size of the files
        long maxSize = 8 * 1024 * 1024; // 8MB

        // If the file is null or empty
        if (file.Length == 0)
        {
            _logger.LogError("[Dashboard controller] FileUpload() failed, error message: file is empty");
            return "";
        }

        // If the file is greater than 8MB
        if (file.Length > maxSize)
        {
            _logger.LogError("[Dashboard controller] FileUpload() failed, error message: file to large");
            return "";
        }

        // Create a new file name with a GUID and the file extension
        var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

        // Create the path to the file
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "categories", fileName);

        // Get the directory name
        var dirName = Path.GetDirectoryName(filePath);

        // If the directory does not exist, create it
        if (!Directory.Exists(dirName))
            try
            {
                // Try to create the directory if it does not exist
                if (dirName != null) Directory.CreateDirectory(dirName);
            }
            catch (Exception e)
            {
                _logger.LogError($"[Dashboard controller] FileUpload() failed, could not create directory. error message: {e}");
                return "";
            }

        // Copy the file to the path
        try
        {
            await using var fs = System.IO.File.Create(filePath);
            await file.CopyToAsync(fs);
            // Source: https://learn.microsoft.com/en-us/dotnet/api/system.io.file.create?view=net-6.0 
        }
        catch (Exception e)
        {
            // Exception and logging if the copying fails
            _logger.LogError("[Dashboard controller] An exception occurred while copying file: {e}", e);
            return "";
        }

        // Return file path
        return Path.Combine("../", "images", "categories", fileName);
    }

    // Function for deleting file
    public bool DeleteFile(string deletePath)
    {
        try
        {
            //Replace the ../ with wwwroot/ to get the correct path
            deletePath = deletePath.Replace("../", "wwwroot/");

            // Check if the file exists, if i does not exist it's probably a external file 
            if (System.IO.File.Exists(deletePath))
            {
                // Delete the file
                System.IO.File.Delete(deletePath);

                // Check if the file got deleted
                if (System.IO.File.Exists(deletePath))
                {
                    _logger.LogError(
                        "[Dashboard controller] DeleteCategory() failed, error message: could not delete file");
                    return false;
                }
                //Source: https://learn.microsoft.com/en-us/dotnet/api/system.io.file.delete?view=net-6.0

                _logger.LogInformation($"[Dashboard controller] File successfully deleted: {deletePath}");
            }
            else
            {
                // Log warning if the file does not exists
                _logger.LogWarning($"[Dashboard controller] File does not exist, skipping deletion: {deletePath}");
            }

            return true;
        }
        catch (Exception e)
        {
            _logger.LogError($"[Dashboard controller] An exception occurred while deleting file: {e}");
            return false;
        }
    }

    // Post request to update an existing tag in the repo and redirects to the admin dashboard
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTag(Tag tag)
    {
        // Checks if the submitted form values passes validation
        if (ModelState.IsValid)
        {
            // Tries to update tag in repo, logs error if it cannot update tag
            var updateTag = await _tagsRepository.Update(tag);
            if (!updateTag)
            {
                TempData["ErrorMessage"] =
                    "Tag update failed."; // Tried creating error message for the user - May just remove it // TODO: Remove this line
                _logger.LogWarning("[Dashboard controller] Tag update failed for {@tag}", tag);
            }
        }

        TempData["TestMessage"] = "This is a test, babe."; // TODO: Remove this line
        // Redirects to admin dashboard
        return RedirectToAction("AdminDashboard");
    }

    // Post request to create a new tag in the repo and redirect to admin dashboard
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> NewTag(Tag tag)
    {
        // Checks if the submitted form values passes validation
        if (ModelState.IsValid)
        {
            // Tries to create tag in repo, logs error if it cannot create tag 
            var newTag = await _tagsRepository.Create(tag);
            if (newTag == null)
                _logger.LogWarning("[Dashboard controller] Tag creation failed for {@tag}", tag);
        }

        // Redirects to admin dashboard
        return RedirectToAction("AdminDashboard");
    }

    // Get request to delete a tag in the repo
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTag(int id)
    {
        // Tries to delete the tag, logs and returns error if there is no category to delete
        var deleteTag = await _tagsRepository.Delete(id);
        if (!deleteTag)
        {
            _logger.LogError("[Dashboard controller] DeleteTag() failed, error message: result is null");
            return NotFound("Category not found");
        }

        // Redirects to admin dashboard
        return RedirectToAction("AdminDashboard");
    }
}