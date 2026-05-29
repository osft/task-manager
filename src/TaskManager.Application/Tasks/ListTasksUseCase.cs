using TaskManager.Application.Tasks.Models;

namespace TaskManager.Application.Tasks;

public sealed class ListTasksUseCase
{
    private readonly ITaskRepository _taskRepository;

    public ListTasksUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public async Task<IReadOnlyList<TaskRecord>> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User id is required.", nameof(userId));
        }

        return await _taskRepository.ListForUserAsync(userId, cancellationToken);
    }
}
