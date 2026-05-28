using Baxture.Api.Dtos;
using Baxture.Api.Models;

namespace Baxture.Api.Services;

public interface IUserService
{
    Task<User?> AuthenticateAsync(string username, string password, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken);
    Task<User> CreateAsync(CreateUserRequest request, string createdBy, CancellationToken cancellationToken);
    Task<User?> UpdateAsync(string id, UpdateUserRequest request, string updatedBy, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken);
    Task<PagedResult<User>> SearchAsync(SearchUsersRequest request, CancellationToken cancellationToken);
}
