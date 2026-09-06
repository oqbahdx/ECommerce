
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.Auth;

namespace ECommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default);
    
    Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default);
    
    Task<RegisterResponse> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
    Task<RefreshTokenResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default);
    Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default);
}