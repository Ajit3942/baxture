using Baxture.Api.Services;

namespace Baxture.Api.Middleware;

public sealed class JwtAuthenticationMiddleware(RequestDelegate next, IJwtTokenService jwtTokenService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            var token = authorization["Bearer ".Length..].Trim();
            var authenticatedUser = jwtTokenService.ValidateToken(token);
            if (authenticatedUser is not null)
            {
                context.SetCurrentUser(authenticatedUser);
            }
        }

        await next(context);
    }
}
