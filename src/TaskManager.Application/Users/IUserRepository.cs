using TaskManager.Application.Users.Models;

namespace TaskManager.Application.Users;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(UserRegistration registration, CancellationToken cancellationToken = default);

    Task<UserRecord?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<UserRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task UpdateLastLoginAsync(Guid userId, DateTime lastLoginUtc, CancellationToken cancellationToken = default);
}
