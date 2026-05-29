using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

/// <summary>
/// Task aggregate root. Instantiate only via <see cref="Factories.ITaskFactory"/>.
/// </summary>
public sealed class TaskItem
{
    internal TaskItem(
        Guid id,
        Guid userId,
        string title,
        string description,
        int statusId,
        TaskPriority priority,
        DateTime dueDate,
        DateTime createdOn)
    {
        Id = id;
        UserId = userId;
        Title = title;
        Description = description;
        StatusId = statusId;
        Priority = priority;
        DueDate = dueDate;
        CreatedOn = createdOn;
    }

    public Guid Id { get; }

    public Guid UserId { get; }

    public string Title { get; }

    public string Description { get; }

    public int StatusId { get; }

    public TaskPriority Priority { get; }

    public DateTime DueDate { get; }

    public DateTime CreatedOn { get; }
}
