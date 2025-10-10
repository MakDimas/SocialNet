using Microsoft.Extensions.Caching.Memory;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Numerics;

namespace SocialNet.Core.Services.Captcha;

public sealed class CaptchaService : ICaptchaService
{
    private readonly IMemoryCache _cache;
    private static readonly char[] Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789".ToCharArray();

    public CaptchaService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public (string CaptchaId, byte[] ImagePng) GenerateCaptcha(TimeSpan lifetime)
    {
        string text = GenerateText(6);
        string id = Guid.NewGuid().ToString("N");

        _cache.Set(GetKey(id), text, lifetime);

        byte[] imageBytes = RenderImage(text);
        return (id, imageBytes);
    }

    public bool ValidateCaptcha(string captchaId, string userInput, bool removeOnSuccess = true)
    {
        if (string.IsNullOrWhiteSpace(captchaId) || string.IsNullOrWhiteSpace(userInput)) return false;
        if (!_cache.TryGetValue<string>(GetKey(captchaId), out var expected)) return false;
        var ok = string.Equals(expected, userInput.Trim(), StringComparison.OrdinalIgnoreCase);
        if (ok && removeOnSuccess) _cache.Remove(GetKey(captchaId));
        return ok;
    }

    private static string GetKey(string id) => $"captcha:{id}";

    private static string GenerateText(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];
        for (int i = 0; i < length; i++)
        {
            chars[i] = Alphabet[bytes[i] % Alphabet.Length];
        }
        return new string(chars);
    }

    private static byte[] RenderImage(string text)
    {
        const int width = 220;
        const int height = 60;

        static int NextInt(int max) => RandomNumberGenerator.GetInt32(max);

        using var image = new Image<Rgba32>(width, height);

        var backgroundColor = Color.FromRgb(0xEE, 0xF2, 0xF7);
        image.Mutate(ctx => ctx.Fill(backgroundColor));

        image.Mutate(ctx =>
        {
            for (int i = 0; i < 6; i++)
            {
                var color = Color.FromRgb(
                    (byte)(150 + NextInt(50)),
                    (byte)(150 + NextInt(50)),
                    (byte)(150 + NextInt(50)));
                var pointCount = 4 + NextInt(4);
                var points = new PointF[pointCount];
                for (int p = 0; p < pointCount; p++)
                {
                    points[p] = new PointF(NextInt(width), NextInt(height));
                }
                var builder = new PathBuilder();
                builder.AddLines(points);
                var path = builder.Build();
                ctx.Draw(color, 1f, path);
            }
        });

        var font = SystemFonts.CreateFont("DejaVu Sans", 30, SixLabors.Fonts.FontStyle.Bold);
        var textColor = Color.FromRgb(70, 70, 90);

        int leftMargin = 10;
        int rightMargin = 10;
        float step = (width - leftMargin - rightMargin) / (float)text.Length;
        float cy = height / 2f;

        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            float cx = leftMargin + step * i + step / 2f;
            float rotation = NextInt(21) - 10;

            var glyphPath = TextBuilder.GenerateGlyphs(character.ToString(), new RichTextOptions(font)
            {
                Dpi = 96
            });

            var bounds = glyphPath.Bounds;
            var centerX = bounds.X + bounds.Width / 2f;
            var centerY = bounds.Y + bounds.Height / 2f;

            var translateToCell = Matrix3x2.CreateTranslation(cx - centerX, cy - centerY);
            var rotateAroundCenter = Matrix3x2.CreateRotation((float)(Math.PI * rotation / 180.0), new Vector2(cx, cy));
            var finalTransform = translateToCell * rotateAroundCenter;

            var transformed = glyphPath.Transform(finalTransform);
            image.Mutate(ctx => ctx.Fill(textColor, transformed));
        }

        image.Mutate(ctx =>
        {
            for (int i = 0; i < 8; i++)
            {
                int w = 40 + NextInt(40);
                int h = 20 + NextInt(20);
                int x = NextInt(Math.Max(1, width - w));
                int y = NextInt(Math.Max(1, height - h));

                var ellipse = new EllipsePolygon(x + w / 2f, y + h / 2f, w / 2f, h / 2f);
                ctx.Draw(Color.FromRgb(100, 120, 130), 1f, ellipse);
            }
        });

        using var ms = new MemoryStream();
        image.SaveAsPng(ms);
        return ms.ToArray();
    }
}
