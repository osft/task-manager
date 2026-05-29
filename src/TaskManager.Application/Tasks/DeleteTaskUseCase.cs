using TaskManager.Application.Exceptions;
using TaskManager.Application.Tasks.Commands;

namespace TaskManager.Application.Tasks;

public sealed class DeleteTaskUseCase
{
    private readonly ITaskRepository _taskRepository;

    public DeleteTaskUseCase(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public async Task ExecuteAsync(DeleteTaskCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var deleted = await _taskRepository.DeleteAsync(command.TaskId, command.UserId, cancellationToken);

        if (!deleted)
        {
            throw new NotFoundException("Task not found.");
        }
    }
}
