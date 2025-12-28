using System.ComponentModel.DataAnnotations;

namespace GameLoggd.Models.Auth;

public class LoginViewModel
{
    [Required]
    [Display(Name = "Email or username")]
    public string Identifier { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
