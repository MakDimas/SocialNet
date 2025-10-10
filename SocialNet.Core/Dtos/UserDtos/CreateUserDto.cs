using System.ComponentModel.DataAnnotations;

namespace SocialNet.Core.Dtos.UserDtos;

public class CreateUserDto
{
    [Required(ErrorMessage = "First name is required")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    public string LastName { get; set; }
    public int Age { get; set; }

    [EmailAddress]
    [Required(ErrorMessage = "Email address is required")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}
