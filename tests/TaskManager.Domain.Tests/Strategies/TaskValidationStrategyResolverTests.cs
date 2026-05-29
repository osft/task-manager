using FluentAssertions;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Strategies;

namespace TaskManager.Domain.Tests.Strategies;

public class TaskValidationStrategyResolverTests
{
    private readonly TaskValidationStrategyResolver _sut = new();

    [Fact]
    public void Resolve_WhenHigh_ReturnsHighPriorityStrategy()
    {
        _sut.Resolve(TaskPriority.High).Should().BeOfType<HighPriorityDueDateStrategy>();
    }

    [Fact]
    public void Resolve_WhenStandard_ReturnsStandardLowStrategy()
    {
        _sut.Resolve(TaskPriority.Standard).Should().BeOfType<StandardLowPriorityDueDateStrategy>();
    }

    [Fact]
    public void Resolve_WhenLow_ReturnsStandardLowStrategy()
    {
        _sut.Resolve(TaskPriority.Low).Should().BeOfType<StandardLowPriorityDueDateStrategy>();
    }
}
