using SocialNet.Core.Dtos.UserDtos;

namespace SocialNet.Core.Services.Users;

public interface IUserService
{
    Task CreateUserAsync(CreateUserDto userDto);
}
