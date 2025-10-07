using Microsoft.Extensions.Caching.Memory;

namespace SocialNet.Core.Services.Captcha;

public interface ICaptchaService
{
    (string CaptchaId, byte[] ImagePng) GenerateCaptcha(TimeSpan lifetime);
    bool ValidateCaptcha(string captchaId, string userInput, bool removeOnSuccess = true);
}


