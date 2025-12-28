using GameLoggd.Models.Domain;

namespace GameLoggd.Models.ViewModels;

public class GameDetailViewModel
{
    public Game Game { get; set; }
    public List<Review> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    // For Review Form
    public int? UserRating { get; set; }
    public string? UserReview { get; set; }
    // For Logging
    public GameLoggd.Models.Domain.UserGameLog? CurrentUserStatus { get; set; }
    public Review? CurrentUserReview { get; set; }
    
    // For 'Add to List' functionality
    public List<GameLoggd.Models.Domain.UserList> CurrentUserLists { get; set; } = new();
}
