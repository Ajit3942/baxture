using Baxture.Api.Models;

namespace Baxture.Api.Dtos;

public sealed record LoginRequest(string Username, string Password);

public sealed record CreateUserRequest(
    string? Username,
    string? Password,
    bool? IsAdmin,
    int? Age,
    List<string>? Hobbies);

public sealed record UpdateUserRequest(
    string? Username,
    string? Password,
    bool? IsAdmin,
    int? Age,
    List<string>? Hobbies);

public sealed record UserResponse(
    string Id,
    string Username,
    bool IsAdmin,
    int Age,
    IReadOnlyCollection<string> Hobbies,
    string CreatedBy,
    DateTime CreatedDate,
    string? UpdatedBy,
    DateTime? UpdatedDate)
{
    public static UserResponse FromUser(User user) =>
        new(
            user.Id,
            user.Username,
            user.IsAdmin,
            user.Age,
            user.Hobbies,
            user.CreatedBy,
            user.CreatedDate,
            user.UpdatedBy,
            user.UpdatedDate);
}

public sealed record UserFilter(string FieldName, string FieldValue);

public sealed record SearchUsersRequest(
    List<UserFilter>? Filters,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = "username",
    string? SortDirection = "asc");

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record ExportUsersRequest(string Format, SearchUsersRequest Search);

public sealed record ExportResult(byte[] Content, string ContentType, string FileName);
