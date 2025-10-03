namespace SocialNet.Core.Dtos.EmailDtos;

public class SmtpSettings
{
    public string Host { get; init; }
    public int Port { get; init; }
    public bool UseSsl { get; init; }
    public string? UserAddress { get; init; }
    public string? AppPassword { get; init; }
}
