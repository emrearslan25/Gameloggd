using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameLoggd.Models.Domain;

public class Review
{
    public int Id { get; set; }
    
    public Guid GameId { get; set; }
    [ForeignKey("GameId")]
    public Game Game { get; set; }

    public string UserId { get; set; } = string.Empty;
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    public ICollection<ReviewComment> Comments { get; set; } = new List<ReviewComment>();
    
    [Range(0, 5)]
    public double Rating { get; set; }

    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public bool IsLikedByCurrentUser { get; set; }
    [NotMapped]
    public int LikeCount { get; set; }
}
