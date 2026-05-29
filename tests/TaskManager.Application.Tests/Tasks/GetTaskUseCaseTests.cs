using FluentAssertions;
using Moq;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Models;

namespace TaskManager.Application.Tests.Tasks;

public class GetTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly GetTaskUseCase _sut;

    public GetTaskUseCaseTests()
    {
        _sut = new GetTaskUseCase(_taskRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskBelongsToUser_ReturnsTask()
    {
        var task = TaskTestData.CreateRecord();
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var result = await _sut.ExecuteAsync(TaskTestData.TaskId, TaskTestData.UserId);

        result.Should().BeSameAs(task);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotFoundForUser_ThrowsNotFoundException()
    {
        _taskRepository
            .Setup(r => r.GetByIdForUserAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskRecord?)null);

        var act = () => _sut.ExecuteAsync(TaskTestData.TaskId, TaskTestData.UserId);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
