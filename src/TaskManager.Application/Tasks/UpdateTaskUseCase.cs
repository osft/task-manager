using TaskManager.Application.Exceptions;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Models;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Strategies;

namespace TaskManager.Application.Tasks;

public sealed class UpdateTaskUseCase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ITaskStatusRepository _taskStatusRepository;
    private readonly ITaskDueDateValidator _dueDateValidator;

    public UpdateTaskUseCase(
        ITaskRepository taskRepository,
        ITaskStatusRepository taskStatusRepository,
        ITaskDueDateValidator dueDateValidator)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _taskStatusRepository = taskStatusRepository ?? throw new ArgumentNullException(nameof(taskStatusRepository));
        _dueDateValidator = dueDateValidator ?? throw new ArgumentNullException(nameof(dueDateValidator));
    }

    public async Task<TaskRecord> ExecuteAsync(UpdateTaskCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var existing = await _taskRepository.GetByIdForUserAsync(command.TaskId, command.UserId, cancellationToken);

        if (existing is null)
        {
            throw new NotFoundException("Task not found.");
        }

        if (string.IsNullOrWhiteSpace(command.Title))
        {
            throw new DomainValidationException("Task title is required.");
        }

        var status = await _taskStatusRepository.GetByIdAsync(command.StatusId, cancellationToken);

        if (status is null || !status.IsActive)
        {
            throw new DomainValidationException("Invalid or inactive task status.");
        }

        var validationResult = _dueDateValidator.Validate(
            command.Priority,
            command.DueDate,
            existing.CreatedOnUtc);

        if (!validationResult.IsSuccess)
        {
            throw new DomainValidationException(validationResult.ErrorMessage!);
        }

        var updatedOn = DateTime.UtcNow;
        var updated = new TaskRecord
        {
            Id = existing.Id,
            UserId = existing.UserId,
            Title = command.Title.Trim(),
            Description = command.Description?.Trim() ?? string.Empty,
            StatusId = command.StatusId,
            Priority = command.Priority,
            DueDate = command.DueDate,
            CreatedBy = existing.CreatedBy,
            CreatedOnUtc = existing.CreatedOnUtc,
            UpdatedBy = command.UserId,
            UpdatedOnUtc = updatedOn
        };

        var result = await _taskRepository.UpdateAsync(updated, cancellationToken);

        if (result is null)
        {
            throw new NotFoundException("Task not found.");
        }

        return result;
    }
}
