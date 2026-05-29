using FluentAssertions;
using Moq;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Models;

namespace TaskManager.Application.Tests.Tasks;

public class ListTasksUseCaseTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly ListTasksUseCase _sut;

    public ListTasksUseCaseTests()
    {
        _sut = new ListTasksUseCase(_taskRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsTasksForUser()
    {
        IReadOnlyList<TaskRecord> tasks = new[] { TaskTestData.CreateRecord() };
        _taskRepository
            .Setup(r => r.ListForUserAsync(TaskTestData.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tasks);

        var result = await _sut.ExecuteAsync(TaskTestData.UserId);

        result.Should().BeEquivalentTo(tasks);
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIdEmpty_ThrowsArgumentException()
    {
        var act = () => _sut.ExecuteAsync(Guid.Empty);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
