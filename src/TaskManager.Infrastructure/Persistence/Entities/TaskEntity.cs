using TaskManager.Domain.Enums;

namespace TaskManager.Infrastructure.Persistence.Entities;

public sealed class TaskEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int StatusId { get; set; }

    public TaskPriority Priority { get; set; }

    public DateTime DueDate { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedOn { get; set; }

    public UserEntity User { get; set; } = null!;

    public TaskStatusEntity Status { get; set; } = null!;
}
