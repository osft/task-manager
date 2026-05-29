using TaskManager.Domain.Enums;
using TaskManager.Domain.Validation;

namespace TaskManager.Domain.Strategies;

/// <summary>
/// Validates task due dates using priority-based strategies.
/// Use for updates anchored to the task's original <c>CreatedOn</c>.
/// </summary>
public interface ITaskDueDateValidator
{
    ValidationResult Validate(TaskPriority priority, DateTime dueDate, DateTime createdOnUtc);
}
