using System.ComponentModel.DataAnnotations;

namespace depiR5gp.ViewModels;

public class LoginVM
{
    [Required(ErrorMessage = "Email or username is required.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;

    public string? ReturnUrl { get; set; }
}

