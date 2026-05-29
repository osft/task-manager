namespace TaskManager.Infrastructure.Persistence.Entities;

public sealed class UserEntity
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Alias { get; set; }

    public string? PasswordHash { get; set; }

    public int RoleId { get; set; }

    public bool IsActive { get; set; }

    public DateTime? EndDate { get; set; }

    public DateTime? LastLoginDate { get; set; }

    public bool MfaEnabled { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedOn { get; set; }

    public RoleEntity Role { get; set; } = null!;

    public ICollection<TaskEntity> Tasks { get; set; } = new List<TaskEntity>();
}
