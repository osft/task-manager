using FluentAssertions;
using TaskManager.Domain.Strategies;

namespace TaskManager.Domain.Tests.Strategies;

public class StandardLowPriorityDueDateStrategyTests
{
    private readonly StandardLowPriorityDueDateStrategy _sut = new();
    private static readonly DateTime CreatedAt = new(2026, 5, 28, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_WhenDueDateIs6MonthsAfterCreation_ReturnsSuccess()
    {
        var dueDate = CreatedAt.AddMonths(6);

        var result = _sut.Validate(dueDate, CreatedAt);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenDueDateIs366DaysAfterCreation_ReturnsFailure()
    {
        var dueDate = CreatedAt.AddDays(366);

        var result = _sut.Validate(dueDate, CreatedAt);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("one year");
    }

    [Fact]
    public void Validate_WhenDueDateIsInThePast_ReturnsFailure()
    {
        var dueDate = CreatedAt.AddDays(-1);

        var result = _sut.Validate(dueDate, CreatedAt);

        result.IsSuccess.Should().BeFalse();
    }
}
