using Microsoft.AspNetCore.Identity;

namespace GameLoggd.Models;

public class ApplicationUser : IdentityUser
{
    public bool IsBanned { get; set; }

    public string? ProfilePicturePath { get; set; }

    public string? CoverPhotoPath { get; set; }

    [System.ComponentModel.DataAnnotations.MaxLength(500)]
    public string? Bio { get; set; }
}
