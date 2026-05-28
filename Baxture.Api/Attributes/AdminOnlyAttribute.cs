using Baxture.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Baxture.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AdminOnlyAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.GetCurrentUser();
        if (user is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication token is required." });
            return Task.CompletedTask;
        }

        if (!user.IsAdmin)
        {
            context.Result = new ObjectResult(new { message = "Only admin users can access this endpoint." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }

        return Task.CompletedTask;
    }
}
