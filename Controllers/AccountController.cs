using GameLoggd.Models.Auth;
using GameLoggd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using GameLoggd.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GameLoggd.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("/account")]
    public IActionResult Index([FromQuery] string? mode = null, [FromQuery] string? returnUrl = null)
    {
        // mode can be "login" or "register"; default to login
        ViewData["Mode"] = string.Equals(mode, "register", StringComparison.OrdinalIgnoreCase) ? "register" : "login";
        ViewData["ReturnUrl"] = returnUrl;
        ViewData["Title"] = "Account";
        return View(new AccountPageViewModel());
    }

    [HttpPost("/account/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, [FromForm] string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Mode"] = "login";
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Account";
            return View("Index", new AccountPageViewModel { Login = model, Register = new RegisterViewModel() });
        }

        var identifier = (model.Identifier ?? string.Empty).Trim();
        ApplicationUser? user;
        if (identifier.Contains('@'))
        {
            user = await _userManager.FindByEmailAsync(identifier);
        }
        else
        {
            user = await _userManager.FindByNameAsync(identifier);
        }

        if (user is null)
        {
            ModelState.AddModelError(nameof(LoginViewModel.Identifier), "Invalid username/email or password.");
            ViewData["Mode"] = "login";
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Account";
            return View("Index", new AccountPageViewModel { Login = model, Register = new RegisterViewModel() });
        }

        if (user.IsBanned)
        {
            ModelState.AddModelError(nameof(LoginViewModel.Identifier), "This account is banned.");
            ViewData["Mode"] = "login";
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Account";
            return View("Index", new AccountPageViewModel { Login = model, Register = new RegisterViewModel() });
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(nameof(LoginViewModel.Identifier), "Invalid username/email or password.");
            ViewData["Mode"] = "login";
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Account";
            return View("Index", new AccountPageViewModel { Login = model, Register = new RegisterViewModel() });
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost("/account/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, [FromForm] string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Mode"] = "register";
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Account";
            return View("Index", new AccountPageViewModel { Login = new LoginViewModel(), Register = model });
        }

        var user = new ApplicationUser
        {
            UserName = (model.Username ?? string.Empty).Trim(),
            Email = (model.Email ?? string.Empty).Trim(),
        };

        var create = await _userManager.CreateAsync(user, model.Password);
        if (!create.Succeeded)
        {
            foreach (var error in create.Errors)
            {
                var field = error.Code switch
                {
                    "DuplicateUserName" => nameof(RegisterViewModel.Username),
                    "DuplicateEmail" => nameof(RegisterViewModel.Email),
                    _ => string.Empty
                };
                ModelState.AddModelError(field, error.Description);
            }

            ViewData["Mode"] = "register";
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Account";
            return View("Index", new AccountPageViewModel { Login = new LoginViewModel(), Register = model });
        }

        await _signInManager.SignInAsync(user, isPersistent: false);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("/account/manage")]
    [Authorize]
    public async Task<IActionResult> Manage()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var model = new ManageProfileViewModel
        {
            Username = user.UserName,
            Email = user.Email,
            Bio = user.Bio,
            ProfilePicturePath = user.ProfilePicturePath,
            CoverPhotoPath = user.CoverPhotoPath,
            StatusMessage = TempData["StatusMessage"] as string
        };

        return View(model);
    }

    [HttpPost("/account/manage/update")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ManageProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("User not found.");

        if (!ModelState.IsValid)
        {
            return View("Manage", model);
        }

        var username = user.UserName;
        if (model.Username != username)
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.Username);
            if (!setUserNameResult.Succeeded)
            {
                foreach (var error in setUserNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Manage", model);
            }
        }

        var email = user.Email;
        if (model.Email != email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Manage", model);
            }
        }

        // Update Bio
        if (model.Bio != user.Bio)
        {
            user.Bio = model.Bio;
            await _userManager.UpdateAsync(user);
        }

        // Update Profile Picture
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
             var ext = Path.GetExtension(model.ProfilePicture.FileName).ToLowerInvariant();
             var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
             
             if (allowed.Contains(ext))
             {
                 var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");
                 Directory.CreateDirectory(uploadsDir);

                 var fileName = $"{user.Id}_{Guid.NewGuid():N}{ext}";
                 var physicalPath = Path.Combine(uploadsDir, fileName);

                 using (var stream = System.IO.File.Create(physicalPath))
                 {
                     await model.ProfilePicture.CopyToAsync(stream);
                 }

                 // Delete old if exists
                 if (!string.IsNullOrEmpty(user.ProfilePicturePath))
                 {
                     var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePicturePath.TrimStart('/'));
                     if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                 }

                 user.ProfilePicturePath = $"/uploads/avatars/{fileName}";
                 await _userManager.UpdateAsync(user);
             }
        }

        // Update Cover Photo
        if (model.CoverPhoto != null && model.CoverPhoto.Length > 0)
        {
             var ext = Path.GetExtension(model.CoverPhoto.FileName).ToLowerInvariant();
             var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
             
             if (allowed.Contains(ext))
             {
                 var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "covers");
                 Directory.CreateDirectory(uploadsDir);

                 var fileName = $"{user.Id}_cover_{Guid.NewGuid():N}{ext}";
                 var physicalPath = Path.Combine(uploadsDir, fileName);

                 using (var stream = System.IO.File.Create(physicalPath))
                 {
                     await model.CoverPhoto.CopyToAsync(stream);
                 }

                 // Delete old if exists
                 if (!string.IsNullOrEmpty(user.CoverPhotoPath))
                 {
                     var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.CoverPhotoPath.TrimStart('/'));
                     if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                 }

                 user.CoverPhotoPath = $"/uploads/covers/{fileName}";
                 await _userManager.UpdateAsync(user);
             }
        }

        TempData["StatusMessage"] = "Your profile has been updated";
        return RedirectToAction("Manage");
    }

    [HttpPost("/account/manage/password")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ManageProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound("User not found.");

        if (string.IsNullOrEmpty(model.CurrentPassword) || string.IsNullOrEmpty(model.NewPassword))
        {
             ModelState.AddModelError(string.Empty, "Please fill in all password fields.");
             model.Username = user.UserName; 
             model.Email = user.Email;
             return View("Manage", model);
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            model.Username = user.UserName; 
            model.Email = user.Email;
            return View("Manage", model);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "Your password has been changed";
        return RedirectToAction("Manage");
    }

    [HttpPost("/account/logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}

public class AccountPageViewModel
{
    public LoginViewModel Login { get; set; } = new();
    public RegisterViewModel Register { get; set; } = new();
}
