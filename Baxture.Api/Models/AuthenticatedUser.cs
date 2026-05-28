namespace Baxture.Api.Models;

public sealed record AuthenticatedUser(string Id, string Username, bool IsAdmin);
