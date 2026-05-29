using FluentAssertions;
using TaskManager.Domain.Strategies;

namespace TaskManager.Domain.Tests.Strategies;

public class HighPriorityDueDateStrategyTests
{
    private readonly HighPriorityDueDateStrategy _sut = new();
    private static readonly DateTime CreatedAt = new(2026, 5, 28, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Validate_WhenDueDateIs47HoursAfterCreation_ReturnsSuccess()
    {
        var dueDate = CreatedAt.AddHours(47);

        var result = _sut.Validate(dueDate, CreatedAt);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Validate_WhenDueDateIs49HoursAfterCreation_ReturnsFailure()
    {
        var dueDate = CreatedAt.AddHours(49);

        var result = _sut.Validate(dueDate, CreatedAt);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("48 hours");
    }

    [Fact]
    public void Validate_WhenDueDateIsInThePast_ReturnsFailure()
    {
        var dueDate = CreatedAt.AddHours(-1);

        var result = _sut.Validate(dueDate, CreatedAt);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("future");
    }

    [Fact]
    public void Validate_WhenDueDateEqualsCreationTime_ReturnsFailure()
    {
        var result = _sut.Validate(CreatedAt, CreatedAt);

        result.IsSuccess.Should().BeFalse();
    }
}
