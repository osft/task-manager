namespace TaskManager.Infrastructure.Persistence.Entities;

public sealed class RoleEntity
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid CreatedBy { get; set; }

    public DateTime CreatedOn { get; set; }

    public Guid UpdatedBy { get; set; }

    public DateTime UpdatedOn { get; set; }

    public ICollection<UserEntity> Users { get; set; } = new List<UserEntity>();
}
