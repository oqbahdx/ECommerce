using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateAccessToken(User user);

    DateTimeOffset GetAccessTokenExpiration();
}