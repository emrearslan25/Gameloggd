using GameLoggd.Models;
using GameLoggd.Models.Domain;

namespace GameLoggd.Models.Admin;

public class AdminDashboardViewModel
{
    public required PaginatedList<Game> Games { get; set; }
    public required PaginatedList<ApplicationUser> Users { get; set; }
    public required PaginatedList<Review> Reviews { get; set; }
    public required PaginatedList<ReviewComment> ReviewComments { get; set; }

    public Game? EditingGame { get; set; }

    public List<Genre> AvailableGenres { get; set; } = new();
    public List<Platform> AvailablePlatforms { get; set; } = new();
    
    public int TotalUsers { get; set; }
    public int TotalGames { get; set; }
    public int TotalReviews { get; set; }
}
