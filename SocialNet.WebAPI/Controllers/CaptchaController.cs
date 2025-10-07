using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialNet.Core.Dtos.Models;
using SocialNet.Core.Services.Captcha;
using System.Net.Mime;

namespace SocialNet.WebAPI.Controllers;

[AllowAnonymous]
public class CaptchaController : BaseController
{
    private readonly ICaptchaService _captchaService;

    public CaptchaController(ICaptchaService captchaService)
    {
        _captchaService = captchaService;
    }

    [HttpGet]
    public ActionResult<CaptchaGenerateResponse> Generate()
    {
        var (id, png) = _captchaService.GenerateCaptcha(TimeSpan.FromMinutes(3));
        var base64 = Convert.ToBase64String(png);

        return Ok(new ActionResultWrapper<CaptchaGenerateResponse>()
        {
            Status = StatusCodes.Status200OK,
            Data = new CaptchaGenerateResponse { CaptchaId = id, ImageBase64 = base64 },
            Message = "Captcha generated successfully"
        });
    }

    [HttpPost]
    public ActionResult<CaptchaValidateResponse> Validate([FromBody] CaptchaValidateRequest request)
    {
        var ok = _captchaService.ValidateCaptcha(request.CaptchaId, request.Input);
        return Ok(new ActionResultWrapper<CaptchaValidateResponse>()
        {
            Status = StatusCodes.Status200OK,
            Data = new CaptchaValidateResponse { Valid = ok },
            Message = "Captcha was validated successfully"
        });
    }
}


