using FluentAssertions;
using Moq;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Application.Tasks.Models;
using TaskManager.Domain.Constants;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Exceptions;
using TaskManager.Domain.Strategies;

namespace TaskManager.Application.Tests.Tasks;

public class UpdateTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<ITaskStatusRepository> _taskStatusRepository = new();
    private readonly UpdateTaskUseCase _sut;

    public UpdateTaskUseCaseTests()
    {
        _taskStatusRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new TaskStatusRecord
            {
                Id = id,
                Name = "Status",
                SortOrder = id,
                IsActive = true
            });

        _sut = new UpdateTaskUseCase(
            _taskRepository.Object,
            _taskStatusRepository.Object,
            new TaskDueDateValidator(new TaskValidationStrategyResolver()));
    }

    [Fact]
    public async Task ExecuteAsync_WhenValid_UpdatesTaskAndPreservesCreatedAudit()
    {
        var existing = TaskTestData.CreateRecord(priority: TaskPriority.Standard);
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        TaskRecord? captured = null;
        _taskRepository
            .Setup(r => r.UpdateAsync(It.IsAny<TaskRecord>(), It.IsAny<CancellationToken>()))
            .Callback<TaskRecord, CancellationToken>((t, _) => captured = t)
            .ReturnsAsync((TaskRecord t, CancellationToken _) => t);

        var newDueDate = TaskTestData.CreatedOn.AddMonths(2);
        await _sut.ExecuteAsync(new UpdateTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId,
            Title = "Updated title",
            Description = "Updated desc",
            Priority = TaskPriority.Standard,
            DueDate = newDueDate,
            StatusId = TaskStatusIds.InProgress
        });

        captured.Should().NotBeNull();
        captured!.Title.Should().Be("Updated title");
        captured.Priority.Should().Be(TaskPriority.Standard);
        captured.DueDate.Should().Be(newDueDate);
        captured.StatusId.Should().Be(TaskStatusIds.InProgress);
        captured.CreatedBy.Should().Be(existing.CreatedBy);
        captured.CreatedOnUtc.Should().Be(existing.CreatedOnUtc);
        captured.UpdatedBy.Should().Be(TaskTestData.UserId);
        captured.UpdatedOnUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskRecord?)null);

        var act = () => _sut.ExecuteAsync(new UpdateTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId,
            Title = "Title",
            Priority = TaskPriority.Standard,
            DueDate = TaskTestData.CreatedOn.AddDays(1),
            StatusId = TaskStatusIds.Todo
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenHighDueDateBeyond48HoursFromCreation_ThrowsDomainValidationException()
    {
        var existing = TaskTestData.CreateRecord(priority: TaskPriority.High, dueDate: TaskTestData.CreatedOn.AddHours(24));
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var act = () => _sut.ExecuteAsync(new UpdateTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId,
            Title = "Still urgent",
            Priority = TaskPriority.High,
            DueDate = TaskTestData.CreatedOn.AddHours(49),
            StatusId = TaskStatusIds.Todo
        });

        await act.Should().ThrowAsync<DomainValidationException>();
        _taskRepository.Verify(r => r.UpdateAsync(It.IsAny<TaskRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTitleEmpty_ThrowsDomainValidationException()
    {
        var existing = TaskTestData.CreateRecord();
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var act = () => _sut.ExecuteAsync(new UpdateTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId,
            Title = "  ",
            Priority = TaskPriority.Standard,
            DueDate = TaskTestData.CreatedOn.AddMonths(1),
            StatusId = TaskStatusIds.Todo
        });

        await act.Should().ThrowAsync<DomainValidationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatusDoesNotExist_ThrowsDomainValidationException()
    {
        var existing = TaskTestData.CreateRecord();
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _taskStatusRepository
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskStatusRecord?)null);

        var act = () => _sut.ExecuteAsync(new UpdateTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId,
            Title = "Title",
            Priority = TaskPriority.Standard,
            DueDate = TaskTestData.CreatedOn.AddMonths(1),
            StatusId = 999
        });

        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("*status*");
        _taskRepository.Verify(r => r.UpdateAsync(It.IsAny<TaskRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStatusInactive_ThrowsDomainValidationException()
    {
        var existing = TaskTestData.CreateRecord();
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _taskStatusRepository
            .Setup(r => r.GetByIdAsync(TaskStatusIds.Todo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TaskStatusRecord
            {
                Id = TaskStatusIds.Todo,
                Name = "Todo",
                SortOrder = 1,
                IsActive = false
            });

        var act = () => _sut.ExecuteAsync(new UpdateTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId,
            Title = "Title",
            Priority = TaskPriority.Standard,
            DueDate = TaskTestData.CreatedOn.AddMonths(1),
            StatusId = TaskStatusIds.Todo
        });

        await act.Should().ThrowAsync<DomainValidationException>()
            .WithMessage("*status*");
    }

    [Fact]
    public async Task ExecuteAsync_WhenUpdateReturnsNull_ThrowsNotFoundException()
    {
        var existing = TaskTestData.CreateRecord();
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _taskRepository
            .Setup(r => r.UpdateAsync(It.IsAny<TaskRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskRecord?)null);

        var act = () => _sut.ExecuteAsync(new UpdateTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId,
            Title = "Title",
            Priority = TaskPriority.Standard,
            DueDate = TaskTestData.CreatedOn.AddMonths(1),
            StatusId = TaskStatusIds.Todo
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
