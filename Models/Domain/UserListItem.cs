using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GameLoggd.Models.Domain;

public class UserListItem
{
    public int Id { get; set; }

    public int UserListId { get; set; }
    [ForeignKey("UserListId")]
    public UserList? UserList { get; set; }

    public Guid GameId { get; set; }
    [ForeignKey("GameId")]
    public Game? Game { get; set; }

    public int Order { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
