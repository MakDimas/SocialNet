namespace SocialNet.Core.Dtos.IdentityDtos;

public class RegisterModel
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; }
    public string LastName { get; set; }
}
