using TaskManager.Domain.Enums;
using TaskManager.Domain.Validation;

namespace TaskManager.Domain.Strategies;

public sealed class TaskDueDateValidator : ITaskDueDateValidator
{
    private readonly ITaskValidationStrategyResolver _strategyResolver;

    public TaskDueDateValidator(ITaskValidationStrategyResolver strategyResolver)
    {
        _strategyResolver = strategyResolver ?? throw new ArgumentNullException(nameof(strategyResolver));
    }

    public ValidationResult Validate(TaskPriority priority, DateTime dueDate, DateTime createdOnUtc)
    {
        var strategy = _strategyResolver.Resolve(priority);
        return strategy.Validate(dueDate, createdOnUtc);
    }
}
