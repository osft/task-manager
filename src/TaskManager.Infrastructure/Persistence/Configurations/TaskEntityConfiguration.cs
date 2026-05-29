using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Persistence.Entities;

namespace TaskManager.Infrastructure.Persistence.Configurations;

internal sealed class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    private static readonly ValueConverter<TaskPriority, string> PriorityConverter = new(
        v => v.ToString(),
        v => Enum.Parse<TaskPriority>(v, true));

    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(e => e.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(e => e.StatusId).HasColumnName("status_id").IsRequired();
        builder.Property(e => e.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20)
            .HasConversion(PriorityConverter)
            .IsRequired();
        builder.Property(e => e.DueDate).HasColumnName("due_date").IsRequired();
        builder.Property(e => e.CreatedBy).HasColumnName("created_by").IsRequired();
        builder.Property(e => e.CreatedOn).HasColumnName("created_on").IsRequired();
        builder.Property(e => e.UpdatedBy).HasColumnName("updated_by").IsRequired();
        builder.Property(e => e.UpdatedOn).HasColumnName("updated_on").IsRequired();

        builder.HasIndex(e => e.UserId).HasDatabaseName("ix_tasks_user_id");
        builder.HasIndex(e => e.StatusId).HasDatabaseName("ix_tasks_status_id");

        builder.HasOne(e => e.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Status)
            .WithMany(s => s.Tasks)
            .HasForeignKey(e => e.StatusId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
