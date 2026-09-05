
using ECommerce.Application.DTOs.auth;

namespace ECommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
}