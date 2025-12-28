using GameLoggd.Models;

namespace GameLoggd.Models.ViewModels;

public class MemberViewModel
{
    public ApplicationUser User { get; set; }
    public bool IsFollowing { get; set; }
}
