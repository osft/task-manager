namespace TaskManager.Application.Tasks.Models;

public sealed class TaskStatusRecord
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public int SortOrder { get; init; }

    public bool IsActive { get; init; }
}
