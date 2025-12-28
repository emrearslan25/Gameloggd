using System;

namespace GameLoggd.Models.Domain;

public class UserListLike
{
    public int UserListId { get; set; }
    public UserList UserList { get; set; } = null!;
    
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
    
    public DateTime LikedAt { get; set; } = DateTime.UtcNow;
}
