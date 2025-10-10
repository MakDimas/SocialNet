using Microsoft.Extensions.DependencyInjection;
using SocialNet.Core.Services.Captcha;
using SocialNet.Core.Services.Emails;
using SocialNet.Core.Services.Identity;
using SocialNet.Core.Services.Posts;
using SocialNet.Core.Services.Token;
using SocialNet.Core.Services.Users;
using SocialNet.Core.Util.ActionTimerSchedulers;

namespace SocialNet.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        services.AddSingleton<IActionTimerScheduler, ActionTimerScheduler>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPostService, PostService>();
        services.AddSingleton<ICaptchaService, CaptchaService>();
        services.AddMemoryCache();

        return services;
    }
}
