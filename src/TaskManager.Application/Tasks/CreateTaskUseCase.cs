using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Models;
using TaskManager.Domain.Factories;

namespace TaskManager.Application.Tasks;

public sealed class CreateTaskUseCase
{
    private readonly ITaskFactory _taskFactory;
    private readonly ITaskRepository _taskRepository;

    public CreateTaskUseCase(ITaskFactory taskFactory, ITaskRepository taskRepository)
    {
        _taskFactory = taskFactory ?? throw new ArgumentNullException(nameof(taskFactory));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public async Task<TaskRecord> ExecuteAsync(CreateTaskCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var task = _taskFactory.Create(
            command.UserId,
            command.Title,
            command.Description ?? string.Empty,
            command.Priority,
            command.DueDate,
            command.CreatedAtUtc);

        return await _taskRepository.CreateAsync(task, command.UserId, cancellationToken);
    }
}
