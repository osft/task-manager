using TaskManager.Application.Tasks.Models;
using TaskManager.Domain.Constants;
using TaskManager.Domain.Enums;

namespace TaskManager.Application.Tests.Tasks;

internal static class TaskTestData
{
    internal static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    internal static readonly Guid OtherUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");
    internal static readonly Guid TaskId = Guid.Parse("22222222-2222-2222-2222-222222222221");
    internal static readonly DateTime CreatedOn = new(2026, 5, 28, 12, 0, 0, DateTimeKind.Utc);

    internal static TaskRecord CreateRecord(
        Guid? id = null,
        Guid? userId = null,
        TaskPriority priority = TaskPriority.Standard,
        DateTime? dueDate = null,
        DateTime? createdOnUtc = null)
    {
        var created = createdOnUtc ?? CreatedOn;
        var user = userId ?? UserId;

        return new TaskRecord
        {
            Id = id ?? TaskId,
            UserId = user,
            Title = "Sample task",
            Description = "Description",
            StatusId = TaskStatusIds.Todo,
            Priority = priority,
            DueDate = dueDate ?? created.AddMonths(1),
            CreatedBy = user,
            CreatedOnUtc = created,
            UpdatedBy = user,
            UpdatedOnUtc = created
        };
    }
}
