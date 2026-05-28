using Baxture.Api.Dtos;
using Baxture.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Baxture.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IUserService userService, IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.AuthenticateAsync(request.Username, request.Password, cancellationToken);
        if (user is null)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }

        return Ok(new
        {
            token = jwtTokenService.CreateToken(user),
            user = UserResponse.FromUser(user)
        });
    }
}
