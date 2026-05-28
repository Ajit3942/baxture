using Baxture.Api.Data;
using Baxture.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Baxture.Api.Repositories;

public sealed class EfUserRepository(AppDbContext dbContext) : IUserRepository
{
    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.Users.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken) =>
        await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public async Task<User?> GetByCredentialsAsync(string username, string password, CancellationToken cancellationToken) =>
        await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(user =>
            user.Username == username && user.Password == password, cancellationToken);

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken)
    {
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<User?> UpdateAsync(string id, Action<User> update, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);
        if (user is null)
        {
            return null;
        }

        update(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(existing => existing.Id == id, cancellationToken);
        if (user is null)
        {
            return false;
        }

        dbContext.Users.Remove(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
