using Microsoft.AspNetCore.Identity;

namespace SocialNet.Domain.Identity;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? Age { get; set; }
    public string? Description { get; set; }
}
