using GameLoggd.Models.Domain;

namespace GameLoggd.Models.ViewModels;

public class HomeIndexViewModel
{
    public Game? HeroGame { get; set; }
    public List<Game> PopularGames { get; set; } = new();
    public List<GameLoggd.Models.Domain.Review> RecentReviews { get; set; } = new();
    public List<ActivityItem> Activities { get; set; } = new();
}
