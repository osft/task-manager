using TaskManager.Application.Tasks.Models;
using TaskManager.Application.Users.Models;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Persistence.Entities;

namespace TaskManager.Infrastructure.Persistence.Mappers;

internal static class PersistenceMappers
{
    public static TaskRecord ToTaskRecord(TaskEntity entity) =>
        new()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Title = entity.Title,
            Description = entity.Description ?? string.Empty,
            StatusId = entity.StatusId,
            StatusName = entity.Status?.Name,
            Priority = entity.Priority,
            DueDate = entity.DueDate,
            CreatedBy = entity.CreatedBy,
            CreatedOnUtc = entity.CreatedOn,
            UpdatedBy = entity.UpdatedBy,
            UpdatedOnUtc = entity.UpdatedOn
        };

    public static TaskEntity ToTaskEntity(TaskItem task, Guid auditUserId)
    {
        var auditTime = task.CreatedOn;
        return new TaskEntity
        {
            Id = task.Id,
            UserId = task.UserId,
            Title = task.Title,
            Description = task.Description,
            StatusId = task.StatusId,
            Priority = task.Priority,
            DueDate = task.DueDate,
            CreatedBy = auditUserId,
            CreatedOn = auditTime,
            UpdatedBy = auditUserId,
            UpdatedOn = auditTime
        };
    }

    public static void ApplyTaskRecord(TaskEntity entity, TaskRecord record)
    {
        entity.Title = record.Title;
        entity.Description = record.Description;
        entity.StatusId = record.StatusId;
        entity.Priority = record.Priority;
        entity.DueDate = record.DueDate;
        entity.UpdatedBy = record.UpdatedBy;
        entity.UpdatedOn = record.UpdatedOnUtc;
    }

    public static UserRecord ToUserRecord(UserEntity entity) =>
        new()
        {
            Id = entity.Id,
            Email = entity.Email,
            Name = entity.Name,
            Alias = entity.Alias,
            PasswordHash = entity.PasswordHash,
            RoleId = entity.RoleId,
            RoleName = entity.Role.Name,
            IsActive = entity.IsActive,
            EndDate = entity.EndDate,
            LastLoginDate = entity.LastLoginDate,
            MfaEnabled = entity.MfaEnabled
        };

    public static TaskStatusRecord ToTaskStatusRecord(TaskStatusEntity entity) =>
        new()
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive
        };
}
