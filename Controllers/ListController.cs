using GameLoggd.Data;
using GameLoggd.Models;
using GameLoggd.Models.Domain;
using GameLoggd.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GameLoggd.Controllers;

[Authorize]
public class ListController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public ListController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    // GET: /lists (My Lists)
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var lists = await _db.UserLists
            .Where(l => l.UserId == user.Id)
            .Include(l => l.Items)
            .ThenInclude(i => i.Game) // Include games for thumbnail preview
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();

        return View(lists);
    }

    // GET: /list/create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /list/create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserList model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Remove UserId from validation as it's set by the server
        ModelState.Remove("UserId");
        ModelState.Remove("User");

        if (ModelState.IsValid)
        {
            model.UserId = user.Id;
            model.CreatedAt = DateTime.UtcNow;
            model.UpdatedAt = DateTime.UtcNow;
            
            _db.UserLists.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }
    
    // GET: /list/details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var list = await _db.UserLists
            .Include(l => l.User)
            .Include(l => l.Items)
            .ThenInclude(i => i.Game)
            .Include(l => l.Likes) // Include likes
            .FirstOrDefaultAsync(l => l.Id == id);

        if (list == null) return NotFound();

        // Sort items by Order or AddedAt
        list.Items = list.Items.OrderBy(i => i.Order).ThenBy(i => i.AddedAt).ToList();
        
        // Like status
        var userId = _userManager.GetUserId(User);
        ViewBag.LikeCount = list.Likes.Count;
        ViewBag.IsLiked = userId != null && list.Likes.Any(l => l.UserId == userId);

        return View(list);
    }
    
    [HttpPost("/list/like")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLike(int listId)
    {
        var list = await _db.UserLists.FindAsync(listId);
        if (list == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Unauthorized();

        var existing = await _db.UserListLikes.FindAsync(listId, userId);
        if (existing != null)
        {
            _db.UserListLikes.Remove(existing);
        }
        else
        {
            _db.UserListLikes.Add(new UserListLike
            {
                UserListId = listId,
                UserId = userId,
                LikedAt = DateTime.UtcNow
            });
        }
        await _db.SaveChangesAsync();

        return RedirectToAction("Details", new { id = listId });
    }

    // POST: /list/delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var list = await _db.UserLists.FirstOrDefaultAsync(l => l.Id == id);
        if (list == null) return NotFound();

        if (list.UserId != user.Id) return Forbid();

        _db.UserLists.Remove(list);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: /list/5/add?query=...
    [HttpGet("/list/{id}/add")]
    public async Task<IActionResult> Add(int id, string? query)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var list = await _db.UserLists.FirstOrDefaultAsync(l => l.Id == id);
        if (list == null) return NotFound();
        if (list.UserId != user.Id) return Forbid();

        var viewModel = new ListAddGameViewModel
        {
            ListId = list.Id,
            ListTitle = list.Title,
            Query = query,
            Results = new List<Game>()
        };

        if (!string.IsNullOrWhiteSpace(query))
        {
            viewModel.Results = await _db.Games
                .AsNoTracking()
                .Where(g => g.Title.ToLower().Contains(query.ToLower()))
                .OrderBy(g => g.Title)
                .Take(20)
                .ToListAsync();
        }

        return View(viewModel);
    }

    // POST: /list/addgame
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddGame(int listId, Guid gameId, string? returnUrl = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var list = await _db.UserLists.Include(l => l.Items).FirstOrDefaultAsync(l => l.Id == listId);
        if (list == null) return NotFound();
        if (list.UserId != user.Id) return Forbid();

        if (list.Items.Any(i => i.GameId == gameId))
        {
            // Already in list
             TempData["StatusMessage"] = "Game already in list.";
        }
        else
        {
            var newItem = new UserListItem
            {
                UserListId = listId,
                GameId = gameId,
                AddedAt = DateTime.UtcNow,
                Order = list.Items.Count + 1
            };
            _db.UserListItems.Add(newItem);
            await _db.SaveChangesAsync();
            TempData["StatusMessage"] = "Game added to list!";
        }

        if (!string.IsNullOrEmpty(returnUrl))
        {
            return Redirect(returnUrl);
        }

        // Redirect back to game page
        var game = await _db.Games.FindAsync(gameId);
        if (game != null) return RedirectToAction("Game", "Home", new { slug = game.Slug });
        
        return RedirectToAction("Index", "Home");
    }
}
