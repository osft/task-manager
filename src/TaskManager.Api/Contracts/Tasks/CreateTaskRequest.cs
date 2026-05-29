using System.ComponentModel.DataAnnotations;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Contracts.Tasks;

public sealed class CreateTaskRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Required]
    public TaskPriority Priority { get; init; }

    [Required]
    public DateTime DueDate { get; init; }
}
