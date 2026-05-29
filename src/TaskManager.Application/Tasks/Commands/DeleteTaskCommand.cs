namespace TaskManager.Application.Tasks.Commands;

public sealed class DeleteTaskCommand
{
    public required Guid TaskId { get; init; }

    public required Guid UserId { get; init; }
}
