using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SocialNet.Core.Dtos.EmailDtos;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using RazorLight;

namespace SocialNet.Core.Services.Emails;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smptSettings;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<SmtpSettings> smptSettings, IConfiguration configuration, ILogger<EmailService> logger)
    {
        _smptSettings = smptSettings.Value;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync<T>(string subject, T model, string templateName, string? email = null) where T : class
    {
        if (!IsValidSmtpSettings()) return;

        var mailboxAddressList = GetMailboxAddressList();

        if (mailboxAddressList is null) return;

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_smptSettings.UserAddress));
        message.Subject = subject;
        message.Body = await GetMessageBodyAsync(model, templateName);
        if (string.IsNullOrEmpty(email))
        {
            message.To.AddRange(mailboxAddressList);
        }
        else
        {
            message.To.Add(MailboxAddress.Parse(email));
        }

        using (var smtpClient = new SmtpClient())
        {
            try
            {
                await smtpClient.ConnectAsync(_smptSettings.Host, _smptSettings.Port, SecureSocketOptions.StartTls);

                await smtpClient.AuthenticateAsync(_smptSettings.UserAddress, _smptSettings.AppPassword);

                await smtpClient.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while sending email: {ex.Message}");
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
            }
        }
    }

    private static async Task<MimeEntity> GetMessageBodyAsync<T>(T model, string templateName) where T : class =>
        new BodyBuilder() { HtmlBody = await GetHtmlBodyAsync(model, templateName) }.ToMessageBody();

    private bool IsValidSmtpSettings() =>
        !string.IsNullOrWhiteSpace(_smptSettings.Host)
            && _smptSettings.Port != 0
            && !string.IsNullOrWhiteSpace(_smptSettings.UserAddress)
            && !string.IsNullOrWhiteSpace(_smptSettings.AppPassword);

    private IEnumerable<MailboxAddress>? GetMailboxAddressList() =>
        _configuration.GetSection("MailboxAddressList").Get<IEnumerable<string>>()?.Select(MailboxAddress.Parse);

    private static async Task<string> GetHtmlBodyAsync<T>(T model, string templateName) where T : class
    {
        var assembly = typeof(EmailService).Assembly;

        var assemblyName = assembly.GetName().Name;

        var templatePath = $"{assemblyName}.Services.Emails.Templates.{templateName}";

        var engine = new RazorLightEngineBuilder().UseEmbeddedResourcesProject(assembly).Build();

        var body = await engine.CompileRenderAsync(templatePath, model);

        return body;
    }
}
