using GameLoggd.Models;

namespace GameLoggd.Models.ViewModels;

public class UserProfileViewModel
{
    public ApplicationUser User { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public bool IsFollowing { get; set; } // If current user is following this profile

    public List<GameLoggd.Models.Domain.Review> Reviews { get; set; } = new();
    public List<GameLoggd.Models.Domain.UserGameLog> PlayingGames { get; set; } = new();
    public List<GameLoggd.Models.Domain.UserGameLog> PlayedGames { get; set; } = new();
    public List<GameLoggd.Models.Domain.UserGameLog> BacklogGames { get; set; } = new();
    public List<GameLoggd.Models.Domain.UserGameLog> WishlistGames { get; set; } = new();
    public List<GameLoggd.Models.Domain.ReviewLike> LikedReviews { get; set; } = new();
    public List<GameLoggd.Models.Domain.UserList> Lists { get; set; } = new();
}
