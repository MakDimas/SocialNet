using Microsoft.AspNetCore.Identity;
using SocialNet.Domain.Posts;

namespace SocialNet.Domain.Identity;

public class User : IdentityUser<int>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int? Age { get; set; }
    public string? Description { get; set; }
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
