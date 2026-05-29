using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Factories;

public interface ITaskFactory
{
    TaskItem Create(
        Guid userId,
        string title,
        string description,
        TaskPriority priority,
        DateTime dueDate,
        DateTime? createdAtUtc = null);
}
