using Baxture.Api.Models;

namespace Baxture.Api.Services;

public static class HttpContextUserExtensions
{
    private const string CurrentUserKey = "CurrentUser";

    public static void SetCurrentUser(this HttpContext context, AuthenticatedUser user) =>
        context.Items[CurrentUserKey] = user;

    public static AuthenticatedUser? GetCurrentUser(this HttpContext context) =>
        context.Items.TryGetValue(CurrentUserKey, out var value) ? value as AuthenticatedUser : null;
}
