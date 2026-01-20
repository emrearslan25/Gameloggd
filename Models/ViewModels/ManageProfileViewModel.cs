using System.ComponentModel.DataAnnotations;

namespace GameLoggd.Models.ViewModels;

public class ManageProfileViewModel
{
    public string Username { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Current Password")]
    public string? CurrentPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "New Password")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
    public string? ConfirmNewPassword { get; set; }

    public string? StatusMessage { get; set; }

    [MaxLength(500)]
    public string? Bio { get; set; }

    public string? ProfilePicturePath { get; set; }

    [DataType(DataType.Upload)]
    [Display(Name = "Profile Picture")]
    public IFormFile? ProfilePicture { get; set; }

    public string? CoverPhotoPath { get; set; }

    [DataType(DataType.Upload)]
    [Display(Name = "Cover Photo")]
    public IFormFile? CoverPhoto { get; set; }

    // Up to 5 favorite games (by Game.Id)
    public Guid?[] FavoriteGameIds { get; set; } = new Guid?[5];

    // UI-only: typed/selected titles for the 5 slots
    public string?[] FavoriteGameTitles { get; set; } = new string?[5];
}
