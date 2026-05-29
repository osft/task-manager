using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tasks.Commands;

public sealed class UpdateTaskCommand
{
    public required Guid TaskId { get; init; }

    public required Guid UserId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required TaskPriority Priority { get; init; }

    public required DateTime DueDate { get; init; }

    public required int StatusId { get; init; }
}
