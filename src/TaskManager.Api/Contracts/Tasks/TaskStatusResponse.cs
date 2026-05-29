namespace TaskManager.Api.Contracts.Tasks;

public sealed class TaskStatusResponse
{
    public required int Id { get; init; }

    public required string Name { get; init; }

    public string? Description { get; init; }

    public int SortOrder { get; init; }
}
