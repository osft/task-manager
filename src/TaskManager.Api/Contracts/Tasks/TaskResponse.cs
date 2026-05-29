using TaskManager.Domain.Enums;

namespace TaskManager.Api.Contracts.Tasks;

public sealed class TaskResponse
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }

    public required int StatusId { get; init; }

    public string? StatusName { get; init; }

    public required TaskPriority Priority { get; init; }

    public required DateTime DueDate { get; init; }

    public required Guid CreatedBy { get; init; }

    public required DateTime CreatedOnUtc { get; init; }

    public required Guid UpdatedBy { get; init; }

    public required DateTime UpdatedOnUtc { get; init; }
}
