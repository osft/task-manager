using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Persistence.Entities;

namespace TaskManager.Infrastructure.Persistence;

public sealed class TaskManagerDbContext : DbContext
{
    public TaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options)
        : base(options)
    {
    }

    public DbSet<RoleEntity> Roles => Set<RoleEntity>();

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<TaskStatusEntity> TaskStatuses => Set<TaskStatusEntity>();

    public DbSet<TaskEntity> Tasks => Set<TaskEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskManagerDbContext).Assembly);
    }
}
