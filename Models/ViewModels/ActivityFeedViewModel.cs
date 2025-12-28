using GameLoggd.Models.Domain;

namespace GameLoggd.Models.ViewModels;

public class ActivityFeedViewModel
{
    public List<ActivityItem> Activities { get; set; } = new();
    
    // Also keep the existing Home content?
    // The user might still want to see "Popular Games" etc.
    // So we can inherit or compose.
    // Let's create a new ViewModel that *contains* the feed, or update HomeIndexViewModel.
}

public class ActivityItem
{
    public ActivityType Type { get; set; }
    public ApplicationUser User { get; set; } = null!;
    public DateTime Date { get; set; }
    
    // Related entities
    public Game? Game { get; set; }
    public Review? Review { get; set; }
    public UserList? List { get; set; }
    public UserGameLog? Log { get; set; }
}

public enum ActivityType
{
    Log,
    Review,
    ListCreated
}
