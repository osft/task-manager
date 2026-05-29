using FluentAssertions;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Strategies;

namespace TaskManager.Domain.Tests.Strategies;

public class TaskDueDateValidatorTests
{
    private readonly TaskDueDateValidator _sut = new(new TaskValidationStrategyResolver());
    private static readonly DateTime CreatedOn = new(2026, 5, 28, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_WhenHighPriorityAtExactly48Hours_ReturnsSuccess()
    {
        var dueDate = CreatedOn.AddHours(48);

        var result = _sut.Validate(TaskPriority.High, dueDate, CreatedOn);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenStandardPriorityAtExactly365Days_ReturnsSuccess()
    {
        var dueDate = CreatedOn.AddDays(365);

        var result = _sut.Validate(TaskPriority.Standard, dueDate, CreatedOn);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenLowPriorityUsesStandardLowRules_ReturnsSuccess()
    {
        var dueDate = CreatedOn.AddDays(30);

        var result = _sut.Validate(TaskPriority.Low, dueDate, CreatedOn);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenUpdateChangesDueDateBeyondWindow_ReturnsFailure()
    {
        // Simulates update on existing task: CreatedOn is fixed, new due date violates High 48h rule
        var result = _sut.Validate(TaskPriority.High, CreatedOn.AddHours(60), CreatedOn);

        result.IsSuccess.Should().BeFalse();
    }
}
