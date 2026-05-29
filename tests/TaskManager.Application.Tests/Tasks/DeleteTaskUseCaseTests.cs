using FluentAssertions;
using Moq;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Commands;

namespace TaskManager.Application.Tests.Tasks;

public class DeleteTaskUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly DeleteTaskUseCase _sut;

    public DeleteTaskUseCaseTests()
    {
        _sut = new DeleteTaskUseCase(_taskRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskExists_Deletes()
    {
        _taskRepository
            .Setup(r => r.DeleteAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.ExecuteAsync(new DeleteTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId
        });

        _taskRepository.Verify(
            r => r.DeleteAsync(TaskTestData.TaskId, TaskTestData.UserId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotFound_ThrowsNotFoundException()
    {
        _taskRepository
            .Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = () => _sut.ExecuteAsync(new DeleteTaskCommand
        {
            TaskId = TaskTestData.TaskId,
            UserId = TaskTestData.UserId
        });

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
