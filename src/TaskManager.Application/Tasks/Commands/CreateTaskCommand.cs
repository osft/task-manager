using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tasks.Commands;

public sealed class CreateTaskCommand
{
    public required Guid UserId { get; init; }

    public required string Title { get; init; }

    public string? Description { get; init; }

    public required TaskPriority Priority { get; init; }

    public required DateTime DueDate { get; init; }

    public DateTime? CreatedAtUtc { get; init; }
}
