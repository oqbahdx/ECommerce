using System.Security.Claims;
using ECommerce.Application.DTOs.auth;
using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest , CancellationToken cancellationToken)
    {
        var response = await authService.RegisterAsync(registerRequest, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(loginRequest, cancellationToken);
        return Ok(response);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }
        
        var user = await authService.GetMeAsync(userId, cancellationToken);
        return Ok(user);

    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("admin-test")]
    public IActionResult AdminTest()
    {
        return Ok(new
        {
            message = "You are an admin."
        });
    }
    
    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await authService.RefreshTokenAsync(
            request,
            cancellationToken);

        return Ok(response);
    }
    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(
            request.RefreshToken,
            cancellationToken);

        return NoContent();
    }
}