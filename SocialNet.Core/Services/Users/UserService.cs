using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Dtos.UserDtos;
using SocialNet.Core.Profiles;
using SocialNet.Core.Repositories;
using SocialNet.Domain.Identity;

namespace SocialNet.Core.Services.Users;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;

    public UserService(IUnitOfWork unitOfWork, ILogger<UserService> logger, UserManager<User> userManager)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _userRepository = _unitOfWork.GetRepository<IUserRepository>();
    }

    public async Task CreateUserAsync(CreateUserDto userDto)
    {
        try
        {
            var userToCreate = new User
            {
                UserName = $"{userDto.FirstName}.{userDto.LastName}",
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Age = userDto.Age
            };

            var createUserResult = await _userManager.CreateAsync(userToCreate, userDto.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException($"User create failed: {errors}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occured while user creating: {ex.Message}");
            throw;
        }
    }

    public async Task<UserResponseDto> GetUserByFullNameAndIdAsync(UserInfoFromUrl userInfo)
    {
        try
        {
            var targetUser = await _userRepository.GetFirstAsync(u => u.UserName == userInfo.FullName && u.Id == userInfo.Id)
                ?? throw new InvalidOperationException($"User was not found");

            return targetUser.FromUserToUserResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occured while getting user: {ex.Message}");
            throw;
        }
    }

    public async Task<UserResponseDto> UpdateUserAsync(UpdateUserDto updateDto)
    {
        try
        {
            var userToUpdate = await _userRepository.GetFirstAsync(u => u.Id == updateDto.Id, u => u.Posts)
                ?? throw new InvalidOperationException($"User was not found");

            await UserDataCheck(userToUpdate, updateDto);

            userToUpdate.UpdateUserInfo(updateDto);

            _userRepository.Update(userToUpdate);
            await _unitOfWork.SaveAsync();

            return userToUpdate.FromUserToUserResponseDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occured while updating user: {ex.Message}");
            throw;
        }
    }

    private async Task UserDataCheck(User userToCheck, UpdateUserDto updateDto)
    {
        if (!string.IsNullOrEmpty(updateDto.Email) && updateDto.Email != userToCheck.Email)
        {
            var emailExists = await _userRepository.GetFirstAsync(
                u => u.Email.ToLower() == updateDto.Email.ToLower() && u.Id != userToCheck.Id);

            if (emailExists is not null)
                throw new InvalidOperationException($"User with entered email already exists");
        }

        string newFirstName = !string.IsNullOrEmpty(updateDto.FirstName)
            ? updateDto.FirstName.Trim()
            : userToCheck.FirstName;

        string newLastName = !string.IsNullOrEmpty(updateDto.LastName)
            ? updateDto.LastName.Trim()
            : userToCheck.LastName;

        if (newFirstName != userToCheck.FirstName || newLastName != userToCheck.LastName)
        {
            var sameUserExists = await _userRepository.GetFirstAsync(
                u => u.FirstName == newFirstName && u.LastName == newLastName && u.Id != userToCheck.Id);

            if (sameUserExists is not null)
                throw new InvalidOperationException($"User with entered first and last name already exists");
        }
    }
}
