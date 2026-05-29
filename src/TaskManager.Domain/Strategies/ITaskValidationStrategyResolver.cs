using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Strategies;

public interface ITaskValidationStrategyResolver
{
    ITaskDueDateValidationStrategy Resolve(TaskPriority priority);
}
