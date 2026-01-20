using GameLoggd.Models;

namespace GameLoggd.Models.Domain;

public class UserFavoriteGame
{
    // 1..5
    public int Slot { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public Guid GameId { get; set; }
    public Game? Game { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
