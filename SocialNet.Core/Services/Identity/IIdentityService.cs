using SocialNet.Core.Dtos.IdentityDtos;
using SocialNet.Domain.Identity;

namespace SocialNet.Core.Services.Identity;

public interface IIdentityService
{
    public Task<string> LoginAsync(LoginModelDto loginModel);
    public Task<string> RegisterUserAsync(RegisterModel model);
    public Task<string> ConfirmEmailAsync(User user, string token);
    public Task<User> FindUserByEmailAsync(string email);
}
