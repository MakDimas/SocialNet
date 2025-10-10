using System.ComponentModel.DataAnnotations;

namespace SocialNet.Core.Dtos.IdentityDtos;

public class RegisterModel
{
    [EmailAddress]
    [Required(ErrorMessage = "Email address is required")]
    public string Email { get; set; } = null!;

    [MinLength(8, ErrorMessage = "Password must contains minimum 8 characters")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "FirstName is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    public string LastName { get; set; }
}
