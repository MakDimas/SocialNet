using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Dtos.UserDtos;

namespace SocialNet.Core.Services.Users;

public interface IUserService
{
    public Task CreateUserAsync(CreateUserDto userDto);
    public Task<UserResponseDto> GetUserByFullNameAndIdAsync(UserInfoFromUrl userInfo);
    public Task<UserResponseDto> UpdateUserAsync(UpdateUserDto updateDto);
}
