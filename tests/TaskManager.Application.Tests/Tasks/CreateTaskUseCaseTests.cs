using FluentAssertions;
using Moq;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Models;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Factories;
using TaskManager.Domain.Strategies;
using DomainTaskFactory = TaskManager.Domain.Factories.TaskFactory;

namespace TaskManager.Application.Tests.Tasks;

public class CreateTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly ITaskFactory _taskFactory = new DomainTaskFactory(new TaskValidationStrategyResolver());
    private readonly CreateTaskUseCase _sut;

    public CreateTaskUseCaseTests()
    {
        _sut = new CreateTaskUseCase(_taskFactory, _taskRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_PersistsViaRepositoryWithAuditUser()
    {
        var expected = TaskTestData.CreateRecord();
        _taskRepository
            .Setup(r => r.CreateAsync(It.IsAny<Domain.Entities.TaskItem>(), TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _sut.ExecuteAsync(new CreateTaskCommand
        {
            UserId = TaskTestData.UserId,
            Title = "New task",
            Description = "Details",
            Priority = TaskPriority.Standard,
            DueDate = TaskTestData.CreatedOn.AddMonths(1),
            CreatedAtUtc = TaskTestData.CreatedOn
        });

        result.Should().BeSameAs(expected);
        _taskRepository.Verify(
            r => r.CreateAsync(
                It.Is<Domain.Entities.TaskItem>(t =>
                    t.UserId == TaskTestData.UserId &&
                    t.Title == "New task" &&
                    t.Priority == TaskPriority.Standard &&
                    t.StatusId == Domain.Constants.TaskStatusIds.Todo &&
                    t.CreatedOn == TaskTestData.CreatedOn),
                TaskTestData.UserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenHighPriorityDueDateBeyond48Hours_ThrowsDomainValidationException()
    {
        var act = () => _sut.ExecuteAsync(new CreateTaskCommand
        {
            UserId = TaskTestData.UserId,
            Title = "Late urgent",
            Priority = TaskPriority.High,
            DueDate = TaskTestData.CreatedOn.AddHours(49),
            CreatedAtUtc = TaskTestData.CreatedOn
        });

        await act.Should().ThrowAsync<DomainValidationException>();
        _taskRepository.Verify(
            r => r.CreateAsync(It.IsAny<Domain.Entities.TaskItem>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStandardDueDateBeyondOneYear_ThrowsDomainValidationException()
    {
        var act = () => _sut.ExecuteAsync(new CreateTaskCommand
        {
            UserId = TaskTestData.UserId,
            Title = "Far future",
            Priority = TaskPriority.Standard,
            DueDate = TaskTestData.CreatedOn.AddYears(1).AddDays(1),
            CreatedAtUtc = TaskTestData.CreatedOn
        });

        await act.Should().ThrowAsync<DomainValidationException>();
        _taskRepository.Verify(
            r => r.CreateAsync(It.IsAny<Domain.Entities.TaskItem>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
