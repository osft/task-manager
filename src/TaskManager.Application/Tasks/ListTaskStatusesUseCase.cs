using TaskManager.Application.Tasks.Models;

namespace TaskManager.Application.Tasks;

public sealed class ListTaskStatusesUseCase
{
    private readonly ITaskStatusRepository _taskStatusRepository;

    public ListTaskStatusesUseCase(ITaskStatusRepository taskStatusRepository)
    {
        _taskStatusRepository = taskStatusRepository ?? throw new ArgumentNullException(nameof(taskStatusRepository));
    }

    public Task<IReadOnlyList<TaskStatusRecord>> ExecuteAsync(CancellationToken cancellationToken = default) =>
        _taskStatusRepository.GetAllActiveAsync(cancellationToken);
}
