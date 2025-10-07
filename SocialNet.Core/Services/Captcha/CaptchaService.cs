using Microsoft.Extensions.Caching.Memory;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Cryptography;

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

        using var bmp = new Bitmap(width, height);
        using var gfx = Graphics.FromImage(bmp);
        gfx.SmoothingMode = SmoothingMode.AntiAlias;
        int Rand(int max) => RandomNumberGenerator.GetInt32(max);

        // background
        gfx.Clear(Color.FromArgb(0xEE, 0xF2, 0xF7));

        // draw noise lines
        using (var pen = new Pen(Color.FromArgb(180, 200, 210), 1))
        {
            for (int i = 0; i < 6; i++)
            {
                pen.Color = Color.FromArgb(150 + Rand(50), 150 + Rand(50), 150 + Rand(50));
                gfx.DrawBezier(pen,
                    new Point(Rand(width), Rand(height)),
                    new Point(Rand(width), Rand(height)),
                    new Point(Rand(width), Rand(height)),
                    new Point(Rand(width), Rand(height)));
            }
        }

        using var path = new GraphicsPath();
        using var font = new Font(FontFamily.GenericSansSerif, 28, FontStyle.Bold);
        var bounds = new Rectangle(10, 5, width - 20, height - 10);
        int x = 20;
        foreach (var ch in text)
        {
            using var tmpPath = new GraphicsPath();
            tmpPath.AddString(ch.ToString(), font.FontFamily, (int)FontStyle.Bold, 36, new Point(x, 10), StringFormat.GenericTypographic);
            float angle = (float)(Rand(21) - 10);
            using var m = new Matrix();
            m.RotateAt(angle, new PointF(x + 12, 30));
            tmpPath.Transform(m);
            path.AddPath(tmpPath, false);
            x += 32;
        }

        using var brush = new SolidBrush(Color.FromArgb(70, 70, 90));
        gfx.FillPath(brush, path);

        using (var pen = new Pen(Color.FromArgb(100, 120, 130), 1))
        {
            for (int i = 0; i < 8; i++)
            {
                gfx.DrawArc(pen, Rand(width - 40), Rand(height - 20), 40 + Rand(40), 20 + Rand(20), Rand(360), Rand(360));
            }
        }

        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}


