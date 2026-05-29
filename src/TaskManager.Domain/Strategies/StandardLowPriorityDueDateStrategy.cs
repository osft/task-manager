using TaskManager.Domain.Validation;

namespace TaskManager.Domain.Strategies;

public sealed class StandardLowPriorityDueDateStrategy : ITaskDueDateValidationStrategy
{
    private static readonly TimeSpan MaxDueWindow = TimeSpan.FromDays(365);

    public ValidationResult Validate(DateTime dueDate, DateTime createdAtUtc)
    {
        if (dueDate <= createdAtUtc)
        {
            return ValidationResult.Failure(
                "Standard and Low priority tasks must have a due date in the future.");
        }

        if (dueDate > createdAtUtc.Add(MaxDueWindow))
        {
            return ValidationResult.Failure(
                "Standard and Low priority tasks cannot be due more than one year from creation.");
        }

        return ValidationResult.Success();
    }
}
