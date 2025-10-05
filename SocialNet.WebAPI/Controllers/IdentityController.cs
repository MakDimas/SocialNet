using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SocialNet.Core.Constants;
using SocialNet.Core.Dtos.IdentityDtos;
using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Options;
using SocialNet.Core.Services.Emails;
using SocialNet.Core.Services.Identity;

namespace SocialNet.WebAPI.Controllers;

[Authorize]
public class IdentityController : BaseController
{
    private readonly string _applicationUrl;
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;

    public IdentityController(IIdentityService identityService, IOptions<FrontendOptions> frontendOptions, IEmailService emailService)
    {
        _emailService = emailService;
        _identityService = identityService;
        _applicationUrl = frontendOptions.Value.ApplicationUrl;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginModelDto loginDto)
    {
        var token = await _identityService.LoginAsync(loginDto);

        var result =
            new ActionResultWrapper<string>()
            {
                Status = StatusCodes.Status200OK,
                Data = token,
                Message = "Token retrieved"
            };

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var token = await _identityService.RegisterUserAsync(model);
        var confirmationUrl = $"{_applicationUrl}/emailconfirmation?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(token)}";

        _ = Task.Run(() => _emailService.SendEmailAsync(
        $"Confirm Your Email Address", confirmationUrl, Templates.RegisterConfirmationTemplate, model.Email));

        var result = new ActionResultWrapper<object>()
        {
            Status = StatusCodes.Status200OK,
            Data = null,
            Message = "Registration successful. Please check your email to confirm your account."
        };

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmDto)
    {
        var user = await _identityService.FindUserByEmailAsync(confirmDto.Email);
        var confirmResult = await _identityService.ConfirmEmailAsync(user, confirmDto.Token);

        var result = new ActionResultWrapper<string>()
        {
            Status = StatusCodes.Status200OK,
            Data = confirmResult,
            Message = "Email confirmed successfully"
        };

        return Ok(result);
    }
}
