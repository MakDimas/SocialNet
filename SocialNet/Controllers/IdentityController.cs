using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocialNet.Core.Constants;
using SocialNet.Core.Dtos.IdentityDtos;
using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Services.Emails;
using SocialNet.Core.Services.Identity;
using SocialNet.Domain.Identity;

namespace SocialNet.WebAPI.Controllers;

[Authorize]
public class IdentityController : BaseController
{
    private readonly IEmailService _emailService;
    private readonly IIdentityService _identityService;

    public IdentityController(IIdentityService identityService, IEmailService emailService)
    {
        _emailService = emailService;
        _identityService = identityService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        var token = await _identityService.RegisterUserAsync(model);
        var confirmationUrl = $"https://localhost:7091/social/Identity/ConfirmEmail?email={Uri.EscapeDataString(model.Email)}&token={Uri.EscapeDataString(token)}";


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

    // Временное решение
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail([FromQuery] string email, [FromQuery] string token)
    {
        var user = await _identityService.FindUserByEmailAsync(email);
        var confirmResult = await _identityService.ConfirmEmailAsync(user, token);

        var result = new ActionResultWrapper<string>()
        {
            Status = StatusCodes.Status200OK,
            Data = confirmResult,
            Message = "Email confirmed successfully"
        };

        return Ok(result);
    }
}
