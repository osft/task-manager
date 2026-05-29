using TaskManager.Application.Tasks.Models;

namespace TaskManager.Application.Tasks;

public interface ITaskStatusRepository
{
    Task<TaskStatusRecord?> GetByIdAsync(int statusId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskStatusRecord>> GetAllActiveAsync(CancellationToken cancellationToken = default);
}
