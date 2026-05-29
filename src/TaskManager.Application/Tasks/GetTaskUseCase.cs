using TaskManager.Application.Exceptions;
using TaskManager.Application.Tasks.Models;

namespace TaskManager.Application.Tasks;

public sealed class GetTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public GetTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public async Task<TaskRecord> ExecuteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var task = await _taskRepository.GetByIdForUserAsync(taskId, userId, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException("Task not found.");
        }

        return task;
    }
}
