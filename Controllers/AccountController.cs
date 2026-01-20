using GameLoggd.Models.Auth;
using GameLoggd.Models;
using GameLoggd.Data;
using Microsoft.EntityFrameworkCore;
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
    private readonly ApplicationDbContext _db;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
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

        // Admins should land in the admin panel, not the normal user area.
        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return Redirect("/admin");
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

        var normalizedUsername = (model.Username ?? string.Empty).Trim();
        if (string.Equals(normalizedUsername, "admin", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(RegisterViewModel.Username), "This username is reserved.");
            ViewData["Mode"] = "register";
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["Title"] = "Account";
            return View("Index", new AccountPageViewModel { Login = new LoginViewModel(), Register = model });
        }

        var user = new ApplicationUser
        {
            UserName = normalizedUsername,
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

        if (await _userManager.IsInRoleAsync(user, "Admin"))
        {
            return Redirect("/admin");
        }

        var favorites = await _db.UserFavoriteGames
            .AsNoTracking()
            .Where(f => f.UserId == user.Id)
            .OrderBy(f => f.Slot)
            .ToListAsync();

        var favoriteIds = new Guid?[5];
        foreach (var fav in favorites)
        {
            if (fav.Slot is >= 1 and <= 5)
            {
                favoriteIds[fav.Slot - 1] = fav.GameId;
            }
        }

        var favoriteTitles = new string?[5];
        var distinctIds = favoriteIds
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (distinctIds.Count > 0)
        {
            var titleMap = await _db.Games
                .AsNoTracking()
                .Where(g => distinctIds.Contains(g.Id))
                .Select(g => new { g.Id, g.Title })
                .ToDictionaryAsync(x => x.Id, x => x.Title);

            for (var i = 0; i < 5; i++)
            {
                var id = favoriteIds[i];
                if (id.HasValue && titleMap.TryGetValue(id.Value, out var title))
                {
                    favoriteTitles[i] = title;
                }
            }
        }

        var model = new ManageProfileViewModel
        {
            Username = user.UserName,
            Email = user.Email,
            Bio = user.Bio,
            ProfilePicturePath = user.ProfilePicturePath,
            CoverPhotoPath = user.CoverPhotoPath,
            StatusMessage = TempData["StatusMessage"] as string,
            FavoriteGameIds = favoriteIds,
            FavoriteGameTitles = favoriteTitles
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
            // Rehydrate titles for any selected IDs so the UI stays stable on validation errors.
            var incomingIdsForUi = model.FavoriteGameIds ?? Array.Empty<Guid?>();
            var distinctIds = incomingIdsForUi
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            if (distinctIds.Count > 0)
            {
                var titleMap = await _db.Games
                    .AsNoTracking()
                    .Where(g => distinctIds.Contains(g.Id))
                    .Select(g => new { g.Id, g.Title })
                    .ToDictionaryAsync(x => x.Id, x => x.Title);

                model.FavoriteGameTitles ??= new string?[5];
                for (var i = 0; i < Math.Min(5, incomingIdsForUi.Length); i++)
                {
                    var id = incomingIdsForUi[i];
                    if (id.HasValue && titleMap.TryGetValue(id.Value, out var title))
                    {
                        model.FavoriteGameTitles[i] = title;
                    }
                }
            }
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

        // Update Favorites (up to 5 slots)
        var incoming = model.FavoriteGameIds ?? Array.Empty<Guid?>();
        var normalizedSlots = new List<(int Slot, Guid GameId)>();
        var seen = new HashSet<Guid>();
        for (var i = 0; i < 5; i++)
        {
            if (i >= incoming.Length) break;
            var gameId = incoming[i];
            if (gameId is null) continue;
            if (!seen.Add(gameId.Value)) continue;
            normalizedSlots.Add((i + 1, gameId.Value));
        }

        if (normalizedSlots.Count > 0)
        {
            var validIds = await _db.Games
                .AsNoTracking()
                .Where(g => normalizedSlots.Select(x => x.GameId).Contains(g.Id))
                .Select(g => g.Id)
                .ToListAsync();

            normalizedSlots = normalizedSlots
                .Where(x => validIds.Contains(x.GameId))
                .ToList();
        }

        var existingFavorites = await _db.UserFavoriteGames.Where(f => f.UserId == user.Id).ToListAsync();
        if (existingFavorites.Count > 0)
        {
            _db.UserFavoriteGames.RemoveRange(existingFavorites);
        }

        foreach (var (slot, gameId) in normalizedSlots)
        {
            _db.UserFavoriteGames.Add(new GameLoggd.Models.Domain.UserFavoriteGame
            {
                UserId = user.Id,
                GameId = gameId,
                Slot = slot,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Your profile has been updated";
        return RedirectToAction("Manage");
    }

    [HttpGet("/account/game-search")]
    [Authorize]
    public async Task<IActionResult> GameSearch([FromQuery] string? q)
    {
        var query = (q ?? string.Empty).Trim();
        if (query.Length < 2)
        {
            return Json(Array.Empty<object>());
        }

        var lower = query.ToLowerInvariant();

        var results = await _db.Games
            .AsNoTracking()
            .Where(g => g.Title != null && EF.Functions.Like(g.Title.ToLower(), $"%{lower}%"))
            .OrderBy(g => g.Title)
            .Take(10)
            .Select(g => new { id = g.Id, title = g.Title })
            .ToListAsync();

        return Json(results);
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
