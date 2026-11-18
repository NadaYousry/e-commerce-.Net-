using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Api.Services;

public interface IJwtTokenService
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}

public sealed class JwtTokenService(IOptions<JwtSettings> settings) : IJwtTokenService
{
    public string GenerateToken(ApplicationUser user, IEnumerable<string> roles)
    {
        var keyMaterial = settings.Value.Key;
        if (string.IsNullOrWhiteSpace(keyMaterial))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var signingKey = Encoding.UTF8.GetBytes(keyMaterial.PadRight(32, '0').Substring(0, 32));
        var credentials = new SigningCredentials(new SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName)
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: settings.Value.Issuer,
            audience: settings.Value.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(settings.Value.ExpiryHours),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
