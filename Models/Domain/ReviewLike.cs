using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameLoggd.Models.Domain;

public class ReviewLike
{
    public int Id { get; set; }

    public int ReviewId { get; set; }
    [ForeignKey("ReviewId")]
    public Review Review { get; set; }

    public string UserId { get; set; }
    [ForeignKey("UserId")]
    public ApplicationUser User { get; set; }

    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}
