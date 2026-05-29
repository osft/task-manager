using TaskManager.Application.Tasks;
using TaskManager.Application.Users;

namespace TaskManager.Application;

/// <summary>
/// Transaction boundary for multi-entity commits. Repositories currently call
/// <c>SaveChangesAsync</c> directly; use this when a use case spans multiple writes.
/// </summary>
public interface IUnitOfWork
{
    ITaskRepository Tasks { get; }

    IUserRepository Users { get; }

    IRoleRepository Roles { get; }

    ITaskStatusRepository TaskStatuses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
