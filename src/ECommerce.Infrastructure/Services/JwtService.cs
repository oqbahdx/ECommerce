using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ECommerce.Application.Common;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce.Infrastructure.Services;

public class JwtService(
    IOptions<JwtOptions> jwtOptions) : IJwtService
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()),

            new(
                JwtRegisteredClaimNames.Email,
                user.Email),

            new(
                ClaimTypes.Role,
                user.Role.ToString()),

            new(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}")
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.SecretKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _options.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    public DateTimeOffset GetAccessTokenExpiration()
    {
        return DateTimeOffset.UtcNow.AddMinutes(
            _options.AccessTokenExpirationMinutes);
    }
}