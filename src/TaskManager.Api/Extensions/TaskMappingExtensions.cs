using TaskManager.Api.Contracts.Tasks;
using TaskManager.Application.Tasks.Models;

namespace TaskManager.Api.Extensions;

internal static class TaskMappingExtensions
{
    internal static TaskResponse ToResponse(this TaskRecord record) =>
        new()
        {
            Id = record.Id,
            Title = record.Title,
            Description = record.Description,
            StatusId = record.StatusId,
            StatusName = record.StatusName,
            Priority = record.Priority,
            DueDate = record.DueDate,
            CreatedBy = record.CreatedBy,
            CreatedOnUtc = record.CreatedOnUtc,
            UpdatedBy = record.UpdatedBy,
            UpdatedOnUtc = record.UpdatedOnUtc
        };

    internal static TaskStatusResponse ToResponse(this TaskStatusRecord record) =>
        new()
        {
            Id = record.Id,
            Name = record.Name,
            Description = record.Description,
            SortOrder = record.SortOrder
        };
}
