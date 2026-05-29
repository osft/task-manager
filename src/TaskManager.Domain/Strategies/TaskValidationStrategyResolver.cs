using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Strategies;

public sealed class TaskValidationStrategyResolver : ITaskValidationStrategyResolver
{
    private readonly HighPriorityDueDateStrategy _highPriorityStrategy = new();
    private readonly StandardLowPriorityDueDateStrategy _standardLowPriorityStrategy = new();

    public ITaskDueDateValidationStrategy Resolve(TaskPriority priority) =>
        priority switch
        {
            TaskPriority.High => _highPriorityStrategy,
            TaskPriority.Standard or TaskPriority.Low => _standardLowPriorityStrategy,
            _ => throw new ArgumentOutOfRangeException(nameof(priority), priority, "Unknown task priority.")
        };
}
