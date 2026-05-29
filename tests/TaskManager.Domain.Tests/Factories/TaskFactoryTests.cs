using FluentAssertions;
using TaskManager.Domain.Constants;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Strategies;
using DomainTaskFactory = TaskManager.Domain.Factories.TaskFactory;

namespace TaskManager.Domain.Tests.Factories;

public class TaskFactoryTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly DateTime CreatedAt = new(2026, 5, 28, 12, 0, 0, DateTimeKind.Utc);

    private readonly DomainTaskFactory _sut = new(new TaskValidationStrategyResolver());

    [Fact]
    public void Create_WithValidHighPriorityTask_ReturnsTaskWithDefaults()
    {
        var dueDate = CreatedAt.AddHours(24);

        var task = _sut.Create(UserId, "Urgent fix", "Description", TaskPriority.High, dueDate, CreatedAt);

        task.Id.Should().NotBe(Guid.Empty);
        task.UserId.Should().Be(UserId);
        task.Title.Should().Be("Urgent fix");
        task.Description.Should().Be("Description");
        task.StatusId.Should().Be(TaskStatusIds.Todo);
        task.Priority.Should().Be(TaskPriority.High);
        task.DueDate.Should().Be(dueDate);
        task.CreatedOn.Should().Be(CreatedAt);
    }

    [Fact]
    public void Create_WithValidStandardPriorityTask_ReturnsTask()
    {
        var dueDate = CreatedAt.AddMonths(3);

        var task = _sut.Create(UserId, "Planning", "", TaskPriority.Standard, dueDate, CreatedAt);

        task.Priority.Should().Be(TaskPriority.Standard);
        task.StatusId.Should().Be(TaskStatusIds.Todo);
    }

    [Fact]
    public void Create_WhenTitleIsEmpty_ThrowsDomainValidationException()
    {
        var act = () => _sut.Create(UserId, "  ", "desc", TaskPriority.Standard, CreatedAt.AddDays(1), CreatedAt);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*title*");
    }

    [Fact]
    public void Create_WhenHighPriorityDueDateBeyond48Hours_ThrowsDomainValidationException()
    {
        var act = () => _sut.Create(
            UserId,
            "Late urgent",
            "desc",
            TaskPriority.High,
            CreatedAt.AddHours(72),
            CreatedAt);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*48 hours*");
    }

    [Fact]
    public void Create_WhenStandardPriorityDueDateBeyondOneYear_ThrowsDomainValidationException()
    {
        var act = () => _sut.Create(
            UserId,
            "Far future",
            "desc",
            TaskPriority.Standard,
            CreatedAt.AddDays(400),
            CreatedAt);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*one year*");
    }

    [Fact]
    public void Create_WhenUserIdIsEmpty_ThrowsDomainValidationException()
    {
        var act = () => _sut.Create(
            Guid.Empty,
            "Title",
            "desc",
            TaskPriority.Low,
            CreatedAt.AddDays(10),
            CreatedAt);

        act.Should().Throw<DomainValidationException>()
            .WithMessage("*User id*");
    }

    [Fact]
    public void Create_TrimsTitleWhitespace()
    {
        var task = _sut.Create(
            UserId,
            "  Trimmed title  ",
            "desc",
            TaskPriority.Low,
            CreatedAt.AddDays(5),
            CreatedAt);

        task.Title.Should().Be("Trimmed title");
    }
}
