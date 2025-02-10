using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce_site.Util;

public class JwtGenerator
{
    private readonly string _symmetricKey;

    public JwtGenerator(IConfiguration config)
    {
        _symmetricKey = config["JWT_KEY"] ?? string.Empty;
    }

    public string GenerateAccessToken(string userId, string email, string role)
    {
        var credential = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_symmetricKey)),
            SecurityAlgorithms.HmacSha256Signature);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role)
        };
        var claimsIdentity = new ClaimsIdentity(claims);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Issuer = "roa.io",
            Audience = "financial-goals-app",
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = credential
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(tokenDescriptor));
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}