using System.ComponentModel.DataAnnotations;

namespace depiR5gp.ViewModels;

public class ForgotPasswordVM
{
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;
}

