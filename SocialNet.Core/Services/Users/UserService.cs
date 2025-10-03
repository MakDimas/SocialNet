using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialNet.Core.Dtos.UserDtos;
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
}
