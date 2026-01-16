using System.ComponentModel.DataAnnotations;

namespace AccountManager.ViewModels.Auth;

public class LoginVm
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

