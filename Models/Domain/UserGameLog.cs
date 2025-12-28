using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameLoggd.Models.Domain;

public enum GameStatus
{
    Playing,
    Played,
    Backlog,
    Wishlist,
    Abandoned
}

public class UserGameLog
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    public Guid GameId { get; set; }
    [ForeignKey("GameId")]
    public Game Game { get; set; }

    public GameStatus Status { get; set; }
    public int? Rating { get; set; } // specific to this log entry, separate from review? 
    // Actually, Review is separate entity now. Let's keep them separate or link them later.
    // For now simple status.
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? DatePlayed { get; set; }
}
