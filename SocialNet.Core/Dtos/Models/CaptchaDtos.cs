namespace SocialNet.Core.Dtos.Models;

public sealed class CaptchaGenerateResponse
{
    public required string CaptchaId { get; init; }
    public required string ImageBase64 { get; init; }
}

public sealed class CaptchaValidateRequest
{
    public required string CaptchaId { get; init; }
    public required string Input { get; init; }
}

public sealed class CaptchaValidateResponse
{
    public required bool Valid { get; init; }
}


