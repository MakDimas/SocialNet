using SocialNet.Domain.Identity;

namespace SocialNet.Core.Services.Token;

public interface ITokenService
{
    public Task<string> GenerateTokenAsync(User user);
}
