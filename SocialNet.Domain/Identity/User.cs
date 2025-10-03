using Microsoft.AspNetCore.Identity;

namespace SocialNet.Domain.Identity;

public class User : IdentityUser<int>
{
    public string Description { get; set; }
}
