using Baxture.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Baxture.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AuthorizeUserAttribute : Attribute, IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.GetCurrentUser() is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Authentication token is required." });
        }

        return Task.CompletedTask;
    }
}
