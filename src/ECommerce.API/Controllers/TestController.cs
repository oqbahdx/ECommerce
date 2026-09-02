using ECommerce.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    [HttpGet("not-found")]
    public IActionResult NotFoundTest()
    {
        throw new NotFoundException("product not found");
    }

    [HttpGet("bad-request")]
    public IActionResult BadRequestTest()
    {
        throw new BadRequestException("bad request");
    }

    [HttpGet("conflict")]
    public IActionResult ConflictTest()
    {
        throw new ConflictException("conflict");
    }

    [HttpGet("server-error")]
    public IActionResult ServerErrorTest()
    {
        throw new Exception("something went wrong");
    }
}