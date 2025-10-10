using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SocialNet.Domain.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SocialNet.Core.Services.Token;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public async Task<string> GenerateTokenAsync(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName)
        };

        if (!string.IsNullOrEmpty(user.PhoneNumber))
            claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));

        if (!string.IsNullOrEmpty(user.Description))
            claims.Add(new Claim("description", user.Description));

        if (user.Age.HasValue)
            claims.Add(new Claim("age", user.Age.Value.ToString()));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
