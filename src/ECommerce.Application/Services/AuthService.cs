using ECommerce.Application.DTOs.auth;
using ECommerce.Application.Exceptions;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Application.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordService passwordService)
    : IAuthService
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
}