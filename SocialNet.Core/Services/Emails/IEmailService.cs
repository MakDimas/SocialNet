namespace SocialNet.Core.Services.Emails;

public interface IEmailService
{
    public Task SendEmailAsync<T>(string subject, T model, string templateName, string? email = null) where T : class;
}
