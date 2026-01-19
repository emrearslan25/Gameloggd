using GameLoggd.Data;
using GameLoggd.Models;
using GameLoggd.Models.Admin;
using GameLoggd.Models.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameLoggd.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AdminController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    [HttpGet("/admin")]
    public async Task<IActionResult> Index([FromQuery] int? gamePage, [FromQuery] int? userPage, [FromQuery] int? reviewPage, [FromQuery] int? commentPage, [FromQuery] Guid? editGameId = null)
    {
        int pageSize = 10;
        
        var gamesQuery = _db.Games.AsNoTracking().OrderByDescending(g => g.CreatedAt);
        var games = await PaginatedList<Game>.CreateAsync(gamesQuery, gamePage ?? 1, pageSize);

        var usersQuery = _db.Users.AsNoTracking().OrderBy(u => u.UserName).AsQueryable();

        // Filter out users in "Admin" role
        var adminRoleId = await _db.Roles.Where(r => r.Name == "Admin").Select(r => r.Id).FirstOrDefaultAsync();
        if (adminRoleId is not null)
        {
            var adminUserIds = _db.UserRoles.Where(ur => ur.RoleId == adminRoleId).Select(ur => ur.UserId);
            usersQuery = usersQuery.Where(u => !adminUserIds.Contains(u.Id));
        }

        var users = await PaginatedList<ApplicationUser>.CreateAsync(usersQuery, userPage ?? 1, pageSize);

        var reviewsQuery = _db.Reviews
            .AsNoTracking()
            .Include(r => r.Game)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt);
        var reviews = await PaginatedList<Review>.CreateAsync(reviewsQuery, reviewPage ?? 1, pageSize);

        var commentsQuery = _db.ReviewComments
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Review)
                .ThenInclude(r => r.Game)
            .OrderByDescending(c => c.CreatedAt);
        var comments = await PaginatedList<ReviewComment>.CreateAsync(commentsQuery, commentPage ?? 1, pageSize);

        Game? editing = null;
        if (editGameId is not null)
        {
            editing = await _db.Games
                .AsNoTracking()
                .Include(g => g.Genres)
                .Include(g => g.Platforms)
                .FirstOrDefaultAsync(g => g.Id == editGameId.Value);
        }

        var allGenres = await _db.Genres.OrderBy(x => x.Name).ToListAsync();
        var allPlatforms = await _db.Platforms.OrderBy(x => x.Name).ToListAsync();

        var totalUsers = await _db.Users.CountAsync();
        var totalGames = await _db.Games.CountAsync();
        var totalReviews = await _db.Reviews.CountAsync();

        return View(new AdminDashboardViewModel
        {
            Games = games,
            Users = users,
            Reviews = reviews,
            ReviewComments = comments,
            EditingGame = editing,
            AvailableGenres = allGenres,
            AvailablePlatforms = allPlatforms,
            TotalUsers = totalUsers,
            TotalGames = totalGames,
            TotalReviews = totalReviews
        });
    }

    [HttpPost("/admin/reviews/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReview([FromForm] int id)
    {
        var review = await _db.Reviews.FirstOrDefaultAsync(r => r.Id == id);
        if (review is null)
        {
            TempData["StatusMessage"] = "Error: Review not found.";
            TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        var likes = await _db.ReviewLikes.Where(l => l.ReviewId == id).ToListAsync();
        if (likes.Count > 0)
        {
            _db.ReviewLikes.RemoveRange(likes);
        }

        _db.Reviews.Remove(review);
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Review deleted successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/review-comments/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteReviewComment([FromForm] int id)
    {
        var comment = await _db.ReviewComments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null)
        {
            TempData["StatusMessage"] = "Error: Comment not found.";
            TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        _db.ReviewComments.Remove(comment);
        await _db.SaveChangesAsync();

        TempData["StatusMessage"] = "Comment deleted successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/games/add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGame(
        [FromForm] string title,
        [FromForm] int? year,
        [FromForm] string developer,
        [FromForm] string description,
        [FromForm] int[] genreIds,
        [FromForm] int[] platformIds,
        IFormFile? image)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["StatusMessage"] = "Error: Game title is required.";
            TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        var imagePath = await SaveGameImageIfAny(image);
        if (image is not null && imagePath is null)
        {
            TempData["StatusMessage"] = "Error: Invalid image. Use png/jpg/webp/gif.";
            TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        // Check for slug uniqueness
        var slug = GenerateSlug(title);
        if (await _db.Games.AnyAsync(g => g.Slug == slug))
        {
             slug += "-" + Guid.NewGuid().ToString().Substring(0, 4);
        }

        var game = new Game
        {
            Title = title.Trim(),
            Year = year,
            Developer = (developer ?? string.Empty).Trim(),
            Description = (description ?? string.Empty).Trim(),
            ImagePath = imagePath,
            Slug = slug
        };

        if (genreIds != null && genreIds.Length > 0)
        {
            var genres = await _db.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            foreach (var g in genres) game.Genres.Add(g);
        }

        if (platformIds != null && platformIds.Length > 0)
        {
            var platforms = await _db.Platforms.Where(p => platformIds.Contains(p.Id)).ToListAsync();
            foreach (var p in platforms) game.Platforms.Add(p);
        }

        _db.Games.Add(game);
        await _db.SaveChangesAsync();
        
        TempData["StatusMessage"] = "Game added successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/games/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateGame(
        [FromForm] Guid id,
        [FromForm] string title,
        [FromForm] int? year,
        [FromForm] string developer,
        [FromForm] string description,
        [FromForm] int[] genreIds,
        [FromForm] int[] platformIds,
        IFormFile? image)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["StatusMessage"] = "Error: Game title is required.";
             TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index), new { editGameId = id });
        }

        var game = await _db.Games
            .Include(g => g.Genres)
            .Include(g => g.Platforms)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (game is null)
        {
            TempData["StatusMessage"] = "Error: Game not found.";
            TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        string? newImagePath = null;
        if (image is not null)
        {
            newImagePath = await SaveGameImageIfAny(image);
            if (newImagePath is null)
            {
                TempData["StatusMessage"] = "Error: Invalid image. Use png/jpg/webp/gif.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index), new { editGameId = id });
            }
        }

        var oldImagePath = game.ImagePath;

        game.Title = title.Trim();
        game.Year = year;
        game.Developer = (developer ?? string.Empty).Trim();
        game.Description = (description ?? string.Empty).Trim();
        
        // Update slug if title changed (or always regenerate but keep ID stable if same?)
        // Better to regenerate and handle collisions if title changed significantly
        var newSlug = GenerateSlug(title);
        if (newSlug != game.Slug && await _db.Games.AnyAsync(g => g.Slug == newSlug && g.Id != id))
        {
             newSlug += "-" + Guid.NewGuid().ToString().Substring(0, 4);
        }
        game.Slug = newSlug;

        if (newImagePath is not null)
        {
            game.ImagePath = newImagePath;
        }

        // Update Genres
        game.Genres.Clear();
        if (genreIds != null && genreIds.Length > 0)
        {
            var genres = await _db.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();
            foreach (var g in genres) game.Genres.Add(g);
        }

        // Update Platforms
        game.Platforms.Clear();
        if (platformIds != null && platformIds.Length > 0)
        {
            var platforms = await _db.Platforms.Where(p => platformIds.Contains(p.Id)).ToListAsync();
            foreach (var p in platforms) game.Platforms.Add(p);
        }

        await _db.SaveChangesAsync();

        if (newImagePath is not null)
        {
            TryDeleteWebFile(oldImagePath);
        }

        TempData["StatusMessage"] = "Game updated successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/games/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGame([FromForm] Guid id)
    {
        var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == id);
        if (game is null)
        {
            TempData["StatusMessage"] = "Error: Game not found.";
            TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        var pathToDelete = game.ImagePath;
        _db.Games.Remove(game);
        await _db.SaveChangesAsync();
        
        // Only delete file after DB success
        TryDeleteWebFile(pathToDelete);
        
        TempData["StatusMessage"] = "Game deleted successfully.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index));
    }



    public static string GenerateSlugStatic(string title)
    {
        if (string.IsNullOrWhiteSpace(title)) return "game-" + Guid.NewGuid().ToString().Substring(0,8);
        
        // Convert to lowercase, replace spaces with hyphens, remove special chars
        var slug = title.ToLowerInvariant().Trim();
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-"); // spaces to hyphens
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", ""); // remove non-alphanumeric (except hyphen)
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\-+", "-"); // collapse hyphens
        slug = slug.Trim('-'); 
        
        return slug;
    }

    private string GenerateSlug(string title) => GenerateSlugStatic(title);

    private async Task<string?> SaveGameImageIfAny(IFormFile? image)
    {
        if (image is null || image.Length == 0) return null;

        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        var allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp", ".gif" };
        if (!allowed.Contains(ext)) return null;

        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "games");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var physicalPath = Path.Combine(uploadsDir, fileName);
        
        using (var stream = System.IO.File.Create(physicalPath))
        {
            await image.CopyToAsync(stream);
        }

        return $"/uploads/games/{fileName}";
    }

    [HttpPost("/admin/users/ban")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser([FromForm] string id)
    {
        if (string.IsNullOrWhiteSpace(id)) 
        {
             TempData["StatusMessage"] = "Error: User not found.";
             TempData["StatusType"] = "danger";
             return RedirectToAction(nameof(Index));
        }
        
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) 
        {
            TempData["StatusMessage"] = "Error: User not found.";
            TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        user.IsBanned = true;
        await _db.SaveChangesAsync();
        
        TempData["StatusMessage"] = "User banned.";
        TempData["StatusType"] = "warning";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("/admin/users/unban")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbanUser([FromForm] string id)
    {
         if (string.IsNullOrWhiteSpace(id)) 
        {
             TempData["StatusMessage"] = "Error: User not found.";
             TempData["StatusType"] = "danger";
             return RedirectToAction(nameof(Index));
        }
        
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) 
        {
            TempData["StatusMessage"] = "Error: User not found.";
            TempData["StatusType"] = "danger";
            return RedirectToAction(nameof(Index));
        }

        user.IsBanned = false;
        await _db.SaveChangesAsync();
        
        TempData["StatusMessage"] = "User unbanned.";
        TempData["StatusType"] = "success";
        return RedirectToAction(nameof(Index));
    }

    private void TryDeleteWebFile(string? webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath)) return;
        if (!webPath.StartsWith('/')) return;

        var relative = webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var physical = Path.Combine(_env.WebRootPath, relative);

        try
        {
            if (System.IO.File.Exists(physical))
            {
                System.IO.File.Delete(physical);
            }
        }
        catch
        {
            // best-effort
        }
    }
}
