using System.ComponentModel.DataAnnotations;

public class LoginRequest
{
    [Required(ErrorMessage = "required")]
    [EmailValidation]
    public string Email { get; set; } = "";

    public string? Password { get; set; }
}