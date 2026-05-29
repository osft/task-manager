namespace TaskManager.Domain.Constants;

/// <summary>
/// Maps to seeded <c>task_statuses</c> rows in PostgreSQL.
/// </summary>
public static class TaskStatusIds
{
    public const int Todo = 1;
    public const int InProgress = 2;
    public const int Done = 3;
}
