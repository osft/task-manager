using TaskManager.Application;
using TaskManager.Application.Tasks;
using TaskManager.Application.Users;

namespace TaskManager.Infrastructure.Persistence;

public sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly TaskManagerDbContext _context;

    public EfUnitOfWork(
        TaskManagerDbContext context,
        ITaskRepository tasks,
        IUserRepository users,
        IRoleRepository roles,
        ITaskStatusRepository taskStatuses)
    {
        _context = context;
        Tasks = tasks;
        Users = users;
        Roles = roles;
        TaskStatuses = taskStatuses;
    }

    public ITaskRepository Tasks { get; }

    public IUserRepository Users { get; }

    public IRoleRepository Roles { get; }

    public ITaskStatusRepository TaskStatuses { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
