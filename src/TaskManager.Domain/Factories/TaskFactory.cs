using TaskManager.Domain.Constants;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Strategies;

namespace TaskManager.Domain.Factories;

public sealed class TaskFactory : ITaskFactory
{
    private readonly ITaskValidationStrategyResolver _strategyResolver;

    public TaskFactory(ITaskValidationStrategyResolver strategyResolver)
    {
        _strategyResolver = strategyResolver ?? throw new ArgumentNullException(nameof(strategyResolver));
    }

    public TaskItem Create(
        Guid userId,
        string title,
        string description,
        TaskPriority priority,
        DateTime dueDate,
        DateTime? createdAtUtc = null)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainValidationException("User id is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainValidationException("Task title is required.");
        }

        var createdOn = createdAtUtc ?? DateTime.UtcNow;
        var strategy = _strategyResolver.Resolve(priority);
        var validationResult = strategy.Validate(dueDate, createdOn);

        if (!validationResult.IsSuccess)
        {
            throw new DomainValidationException(validationResult.ErrorMessage!);
        }

        return new TaskItem(
            Guid.NewGuid(),
            userId,
            title.Trim(),
            description?.Trim() ?? string.Empty,
            TaskStatusIds.Todo,
            priority,
            dueDate,
            createdOn);
    }
}
