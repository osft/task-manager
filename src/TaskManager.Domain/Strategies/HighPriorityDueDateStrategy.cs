using TaskManager.Domain.Validation;

namespace TaskManager.Domain.Strategies;

public sealed class HighPriorityDueDateStrategy : ITaskDueDateValidationStrategy
{
    private static readonly TimeSpan MaxDueWindow = TimeSpan.FromHours(48);

    public ValidationResult Validate(DateTime dueDate, DateTime createdAtUtc)
    {
        if (dueDate <= createdAtUtc)
        {
            return ValidationResult.Failure("High-priority tasks must have a due date in the future.");
        }

        if (dueDate > createdAtUtc.Add(MaxDueWindow))
        {
            return ValidationResult.Failure(
                "High-priority tasks must be due within 48 hours of creation.");
        }

        return ValidationResult.Success();
    }
}
