using Baxture.Api.Dtos;
using Baxture.Api.Models;
using Baxture.Api.Repositories;

namespace Baxture.Api.Services;

public sealed class UserService(IUserRepository repository) : IUserService
{
    public Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken) =>
        repository.GetByCredentialsAsync(username, password, cancellationToken);

    public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken) =>
        repository.GetAllAsync(cancellationToken);

    public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public Task<User> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = request.Username!.Trim(),
            Password = request.Password!,
            IsAdmin = request.IsAdmin ?? false,
            Age = request.Age!.Value,
            Hobbies = request.Hobbies!
        };

        return repository.CreateAsync(user, cancellationToken);
    }

    public Task<User?> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken) =>
        repository.UpdateAsync(id, user =>
        {
            if (request.Username is not null)
            {
                user.Username = request.Username.Trim();
            }

            if (request.Password is not null)
            {
                user.Password = request.Password;
            }

            if (request.IsAdmin is not null)
            {
                user.IsAdmin = request.IsAdmin.Value;
            }

            if (request.Age is not null)
            {
                user.Age = request.Age.Value;
            }

            if (request.Hobbies is not null)
            {
                user.Hobbies = request.Hobbies;
            }
        }, cancellationToken);

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) =>
        await repository.DeleteAsync(id, cancellationToken);

    public async Task<PagedResult<User>> SearchAsync(SearchUsersRequest request, CancellationToken cancellationToken)
    {
        var users = (await repository.GetAllAsync(cancellationToken)).AsQueryable();

        foreach (var filter in request.Filters ?? [])
        {
            users = ApplyFilter(users, filter);
        }

        users = ApplySort(users, request.SortBy, request.SortDirection);

        var pageNumber = Math.Max(1, request.PageNumber);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var totalCount = users.Count();
        var items = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<User>(items, pageNumber, pageSize, totalCount, totalPages);
    }

    private static IQueryable<User> ApplyFilter(IQueryable<User> users, UserFilter filter)
    {
        var value = filter.FieldValue;
        return filter.FieldName.ToLowerInvariant() switch
        {
            "id" => users.Where(user => user.Id == value),
            "username" => users.Where(user => user.Username.Contains(value, StringComparison.OrdinalIgnoreCase)),
            "password" => users.Where(user => user.Password.Contains(value, StringComparison.OrdinalIgnoreCase)),
            "isadmin" when bool.TryParse(value, out var parsed) => users.Where(user => user.IsAdmin == parsed),
            "age" when int.TryParse(value, out var parsed) => users.Where(user => user.Age == parsed),
            "hobbies" => users.Where(user => user.Hobbies.Any(hobby => hobby.Contains(value, StringComparison.OrdinalIgnoreCase))),
            _ => users
        };
    }

    private static IQueryable<User> ApplySort(IQueryable<User> users, string? sortBy, string? sortDirection)
    {
        var descending = sortDirection?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
        return sortBy?.ToLowerInvariant() switch
        {
            "id" => descending ? users.OrderByDescending(user => user.Id) : users.OrderBy(user => user.Id),
            "age" => descending ? users.OrderByDescending(user => user.Age) : users.OrderBy(user => user.Age),
            "isadmin" => descending ? users.OrderByDescending(user => user.IsAdmin) : users.OrderBy(user => user.IsAdmin),
            _ => descending ? users.OrderByDescending(user => user.Username) : users.OrderBy(user => user.Username)
        };
    }
}
