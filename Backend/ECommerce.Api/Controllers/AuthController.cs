using ECommerce.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var response = await authService.RegisterAsync(request);
            if (response is null)
            {
                return Problem(title: "Registration details are incomplete.", statusCode: StatusCodes.Status400BadRequest);
            }

            return Created(string.Empty, response);
        }
        catch (RequestValidationException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(ex.Errors));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var response = await authService.LoginAsync(request);
        if (response is null)
        {
            return Problem(title: "Invalid email or password.", statusCode: StatusCodes.Status401Unauthorized);
        }

        return Ok(response);
    }
}
