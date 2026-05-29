using TaskManager.Domain.Validation;

namespace TaskManager.Domain.Strategies;

public interface ITaskDueDateValidationStrategy
{
    ValidationResult Validate(DateTime dueDate, DateTime createdAtUtc);
}
