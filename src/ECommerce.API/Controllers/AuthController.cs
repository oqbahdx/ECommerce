using ECommerce.Application.DTOs.auth;
using ECommerce.Application.Interfaces.Services;
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
}