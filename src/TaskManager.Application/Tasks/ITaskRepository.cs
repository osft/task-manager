using TaskManager.Application.Tasks.Models;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Tasks;

public interface ITaskRepository
{
    Task<TaskRecord?> GetByIdForUserAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskRecord>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<TaskRecord> CreateAsync(TaskItem task, Guid auditUserId, CancellationToken cancellationToken = default);

    Task<TaskRecord?> UpdateAsync(TaskRecord task, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default);
}
