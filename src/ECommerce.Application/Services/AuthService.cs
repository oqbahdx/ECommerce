using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using FluentValidation;


namespace ECommerce.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordService passwordService,
    IJwtService jwtService,
    IRefreshTokenService refreshTokenService,
    IValidator<RegisterRequest> registerValidator
) : IAuthService
{
    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        await registerValidator.ValidateAndThrowAsync(
            request,
            cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();

        var emailExists =
            await userRepository.ExistByEmailAsync(
                email,
                cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = email,
            PasswordHash =
                passwordService.HashPassword(
                    request.Password),
            Role = UserRole.Customer,
            IsActive = true
        };

        await userRepository.AddAsync(
            user,
            cancellationToken);

        await userRepository.SaveChangesAsync(
            cancellationToken);

        return new RegisterResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            throw new BadRequestException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new BadRequestException("Account is not active.");
        }

        var passwordValid = passwordService.VerifyHashedPassword(
            user.PasswordHash,
            request.Password
        );

        if (!passwordValid)
        {
            throw new BadRequestException("Invalid email or password.");
        }

        var accessToken = jwtService.GenerateAccessToken(user);
        var refreshToken = refreshTokenService.GenerateToken();
        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenService.HashToken(refreshToken),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };
        await refreshTokenRepository.AddAsync(refreshTokenEntity, cancellationToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);
        var expiresAt = jwtService.GetAccessTokenExpiration();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<RegisterResponse> GetMeAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(
            userId,
            cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(
                "User not found.");
        }

        return new RegisterResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = refreshTokenService.HashToken(
            request.RefreshToken);

        var refreshTokenEntity =
            await refreshTokenRepository.GetByTokenHashAsync(
                tokenHash,
                cancellationToken);

        if (refreshTokenEntity is null)
        {
            throw new BadRequestException(
                "Invalid refresh token.");
        }

        if (refreshTokenEntity.IsRevoked)
        {
            throw new BadRequestException(
                "Refresh token has been revoked.");
        }

        if (refreshTokenEntity.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            throw new BadRequestException(
                "Refresh token has expired.");
        }

        var user = refreshTokenEntity.User;

        if (!user.IsActive)
        {
            throw new BadRequestException(
                "Your account is inactive.");
        }

        // Revoke old refresh token
        refreshTokenEntity.RevokedAt =
            DateTimeOffset.UtcNow;

        refreshTokenRepository.Update(refreshTokenEntity);

        // Generate new tokens
        var newAccessToken =
            jwtService.GenerateAccessToken(user);

        var newRefreshToken =
            refreshTokenService.GenerateToken();

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash =
                refreshTokenService.HashToken(newRefreshToken),
            ExpiresAt =
                DateTimeOffset.UtcNow.AddDays(30)
        };

        await refreshTokenRepository.AddAsync(
            newRefreshTokenEntity,
            cancellationToken);

        await refreshTokenRepository.SaveChangesAsync(
            cancellationToken);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = jwtService.GetAccessTokenExpiration()
        };
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = refreshTokenService.HashToken(refreshToken);

        var refreshTokenEntity =
            await refreshTokenRepository.GetByTokenHashAsync(
                tokenHash,
                cancellationToken);

        if (refreshTokenEntity is null)
        {
            return;
        }

        if (!refreshTokenEntity.IsRevoked)
        {
            refreshTokenEntity.RevokedAt =
                DateTimeOffset.UtcNow;

            refreshTokenRepository.Update(refreshTokenEntity);

            await refreshTokenRepository.SaveChangesAsync(
                cancellationToken);
        }
    }
}