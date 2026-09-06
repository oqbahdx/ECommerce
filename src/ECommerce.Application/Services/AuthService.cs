using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtService jwtService
) : IAuthService
{
    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
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
        var expiresAt = jwtService.GetAccessTokenExpiration();

        return new LoginResponse
        {
            AccessToken = accessToken,
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
}