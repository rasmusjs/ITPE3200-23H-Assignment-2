// This file is largely unchanged from the original files in Areas/Identity/Pages 

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using forum.Models;

namespace forum.Controllers;

using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AccountController : Controller
{
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;


    public AccountController(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        ILogger<RegisterModel> logger
    )
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
    }


    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");
        return Ok("Logged out successfully");
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        // Custom validation for username, anonymous is reserved for deleted users
        if (model.UserName.ToLower() == "anonymous")
        {
            return StatusCode(422, "Username is not allowed");
        }

        if (ModelState.IsValid)
        {
            var user = CreateUser();

            model.UserName = model.UserName.ToLower(); // Convert username to lowercase

            await _userStore.SetUserNameAsync(user, model.UserName, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Add role to the user
                var resultRole = await _userManager.AddToRoleAsync(user, "User");
                if (!resultRole.Succeeded) _logger.LogInformation("User could not be added to role.");

                await _signInManager.SignInAsync(user, false);
                return Ok("Registered successfully");
            }
        }

        return BadRequest("Invalid registration attempt");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginModel model)
    {
        // The following codeblock is from https://stackoverflow.com/questions/75991569/how-to-login-with-either-username-or-email-in-an-asp-net-core-6-0 
        if (model.Identifier.Contains('@')) // If the username contains an @ symbol, then it is an email
        {
            //Validate email format
            var emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                             @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                             @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            var re = new Regex(emailRegex);
            if (re.IsMatch(model.Identifier))
                // If the email is valid, try to find the username associated with the email
                model.Identifier = _userManager.FindByEmailAsync(model.Identifier).Result.UserName ?? model.Identifier;
            else
                return StatusCode(422, "Email is not valid");
        }
        else // If the username does not contain an @ symbol, then it is a username
        {
            //validate Username format
            var userNameRegex = @"^[a-zA-Z0-9]{3,20}$"; // Only letters and numbers, 3-20 characters
            var re = new Regex(userNameRegex);
            if (!re.IsMatch(model.Identifier))
            {
                return StatusCode(422, "Username is not valid");
            }
        }

        // End of codeblock from https://stackoverflow.com/questions/75991569/how-to-login-with-either-username-or-email-in-an-asp-net-core-6-0 

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Identifier, model.Password, model.RememberMe,
                false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return Ok("Logged in successfully");
            }

            if (result.RequiresTwoFactor)
                return StatusCode(500, "2FA not supported yet");
            //return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, model.RememberMe });

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }

            return StatusCode(401, "Invalid login attempt");
        }

        return BadRequest("Invalid login attempt");
    }


    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                                                $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
            throw new NotSupportedException("The default UI requires a user store with email support.");

        return (IUserEmailStore<ApplicationUser>)_userStore;
    }


    public class RegisterModel
    {
        // Custom field
        [Required]
        [StringLength(20, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Only alphanumeric characters are allowed.")]
        public string UserName { get; set; } = string.Empty;

        [Required] [EmailAddress] public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        // Can be either username or email 
        [Required] public string Identifier { get; set; }  = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}