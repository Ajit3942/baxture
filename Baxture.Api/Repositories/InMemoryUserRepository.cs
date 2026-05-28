using System.Collections.Concurrent;
using Baxture.Api.Models;

namespace Baxture.Api.Repositories;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users = new();

    public InMemoryUserRepository()
    {
        var seedDate = DateTime.UtcNow;
        Seed(new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "admin",
            Password = "admin123",
            IsAdmin = true,
            Age = 35,
            Hobbies = ["architecture", "running"],
            CreatedBy = "system",
            CreatedDate = seedDate
        });

        Seed(new User
        {
            Id = Guid.NewGuid().ToString(),
            Username = "user",
            Password = "user123",
            IsAdmin = false,
            Age = 28,
            Hobbies = ["reading"],
            CreatedBy = "system",
            CreatedDate = seedDate
        });
    }

    public Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyCollection<User>>(_users.Values.Select(Clone).ToList());

    public Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.TryGetValue(id, out var user) ? Clone(user) : null);

    public Task<User?> GetByCredentialsAsync(string username, string password, CancellationToken cancellationToken)
    {
        var user = _users.Values.FirstOrDefault(x =>
            x.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && x.Password == password);

        return Task.FromResult(user is null ? null : Clone(user));
    }

    public Task<User> CreateAsync(User user, CancellationToken cancellationToken)
    {
        _users[user.Id] = Clone(user);
        return Task.FromResult(Clone(user));
    }

    public Task<User?> UpdateAsync(string id, Action<User> update, CancellationToken cancellationToken)
    {
        if (!_users.TryGetValue(id, out var existing))
        {
            return Task.FromResult<User?>(null);
        }

        lock (existing)
        {
            update(existing);
            return Task.FromResult<User?>(Clone(existing));
        }
    }

    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken) =>
        Task.FromResult(_users.TryRemove(id, out _));

    private void Seed(User user) => _users[user.Id] = user;

    private static User Clone(User user) =>
        new()
        {
            Id = user.Id,
            Username = user.Username,
            Password = user.Password,
            IsAdmin = user.IsAdmin,
            Age = user.Age,
            Hobbies = [.. user.Hobbies],
            CreatedBy = user.CreatedBy,
            CreatedDate = user.CreatedDate,
            UpdatedBy = user.UpdatedBy,
            UpdatedDate = user.UpdatedDate
        };
}
