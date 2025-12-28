using System.ComponentModel.DataAnnotations.Schema;

namespace GameLoggd.Models.Domain;

public class UserFollow
{
    public string ObserverId { get; set; } = string.Empty;
    [ForeignKey("ObserverId")]
    public ApplicationUser Observer { get; set; }

    public string TargetId { get; set; } = string.Empty;
    [ForeignKey("TargetId")]
    public ApplicationUser Target { get; set; }

    public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
}
