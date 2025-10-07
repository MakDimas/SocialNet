using Microsoft.Extensions.DependencyInjection;
using SocialNet.Core.Services.Emails;
using SocialNet.Core.Services.Captcha;
using SocialNet.Core.Services.Identity;
using SocialNet.Core.Services.Token;
using SocialNet.Core.Services.Users;
using System.Reflection;
using SocialNet.Core.Services.Posts;

namespace SocialNet.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPostService, PostService>();
        services.AddSingleton<ICaptchaService, CaptchaService>();
        services.AddMemoryCache();

        //services.AddFluentValidationAutoValidation();
        //services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
