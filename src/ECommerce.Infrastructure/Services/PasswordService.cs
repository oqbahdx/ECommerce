using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Identity;

namespace ECommerce.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private readonly IPasswordHasher<object> _passwordHasher;

    public PasswordService()
    {
        _passwordHasher = new PasswordHasher<object>();
    }

    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}