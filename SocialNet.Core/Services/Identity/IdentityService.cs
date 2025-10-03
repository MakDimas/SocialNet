using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SocialNet.Core.Dtos.IdentityDtos;
using SocialNet.Core.Services.Token;
using SocialNet.Domain.Identity;

namespace SocialNet.Core.Services.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<User> userManager, ITokenService tokenService, ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<string> RegisterUserAsync(RegisterModel model)
    {
        try
        {
            var user = new User
            {
                Email = model.Email,
                UserName = model.Email,
                PhoneNumber = model.Phone,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Description = model.Description,
                Age = model.Age
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error occured while user registering: {ex.Message}");
            throw;
        }
    }

    public async Task<string> ConfirmEmailAsync(User user, string token)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            return "Email confirmation failed.";
        }

        return "Email confirmed successfully.";
    }

    public async Task<User> FindUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            throw new InvalidOperationException($"User with email: {email} was not found");
        }

        return user;
    }
}
