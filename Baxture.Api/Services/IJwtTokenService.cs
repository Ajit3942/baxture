using Baxture.Api.Models;

namespace Baxture.Api.Services;

public interface IJwtTokenService
{
    string CreateToken(User user);
    AuthenticatedUser? ValidateToken(string token);
}
