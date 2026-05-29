using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Application.Tasks.Models;
using TaskManager.Domain.Constants;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Factories;
using TaskManager.Domain.Strategies;
using TaskManager.Infrastructure.Persistence.Repositories;
using DomainTaskFactory = TaskManager.Domain.Factories.TaskFactory;

namespace TaskManager.Infrastructure.Tests.Persistence;

[Trait("Category", "Integration")]
public class EfTaskRepositoryIntegrationTests
{
    private static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OtherUserId = Guid.Parse("99999999-9999-9999-9999-999999999999");

    private readonly DomainTaskFactory _taskFactory = new(new TaskValidationStrategyResolver());

    [SkippableFact]
    public async Task CreateAsync_PersistsTaskWithAuditColumnsAndStatusName()
    {
        Skip.IfNot(await PostgresTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");
        await using var context = PostgresTestDatabase.CreateContext();
        var repository = new EfTaskRepository(context, NullLogger<EfTaskRepository>.Instance);
        var createdAt = DateTime.UtcNow;
        var task = _taskFactory.Create(
            DemoUserId,
            $"Integration task {Guid.NewGuid():N}",
            "Created by integration test",
            TaskPriority.Standard,
            createdAt.AddMonths(1),
            createdAt);

        var record = await repository.CreateAsync(task, DemoUserId);

        record.Id.Should().Be(task.Id);
        record.UserId.Should().Be(DemoUserId);
        record.CreatedBy.Should().Be(DemoUserId);
        record.UpdatedBy.Should().Be(DemoUserId);
        record.CreatedOnUtc.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(2));
        record.StatusName.Should().Be("Todo");

        await repository.DeleteAsync(record.Id, DemoUserId);
    }

    [SkippableFact]
    public async Task GetByIdForUserAsync_WhenUserDoesNotOwnTask_ReturnsNull()
    {
        Skip.IfNot(await PostgresTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");
        await using var context = PostgresTestDatabase.CreateContext();
        var repository = new EfTaskRepository(context, NullLogger<EfTaskRepository>.Instance);
        var seededTaskId = Guid.Parse("22222222-2222-2222-2222-222222222221");

        var result = await repository.GetByIdForUserAsync(seededTaskId, OtherUserId);

        result.Should().BeNull();
    }

    [SkippableFact]
    public async Task UpdateAsync_PreservesCreatedAuditFields()
    {
        Skip.IfNot(await PostgresTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");
        await using var context = PostgresTestDatabase.CreateContext();
        var repository = new EfTaskRepository(context, NullLogger<EfTaskRepository>.Instance);
        var createdAt = DateTime.UtcNow;
        var task = _taskFactory.Create(
            DemoUserId,
            $"Update audit {Guid.NewGuid():N}",
            "desc",
            TaskPriority.Standard,
            createdAt.AddMonths(2),
            createdAt);

        var created = await repository.CreateAsync(task, DemoUserId);
        var updatedOn = DateTime.UtcNow;

        var updated = await repository.UpdateAsync(new TaskRecord
        {
            Id = created.Id,
            UserId = created.UserId,
            Title = "Updated title",
            Description = "Updated",
            StatusId = TaskStatusIds.InProgress,
            Priority = TaskPriority.Standard,
            DueDate = created.DueDate,
            CreatedBy = created.CreatedBy,
            CreatedOnUtc = created.CreatedOnUtc,
            UpdatedBy = DemoUserId,
            UpdatedOnUtc = updatedOn
        });

        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated title");
        updated.CreatedBy.Should().Be(created.CreatedBy);
        updated.CreatedOnUtc.Should().Be(created.CreatedOnUtc);
        updated.UpdatedOnUtc.Should().BeCloseTo(updatedOn, TimeSpan.FromSeconds(2));
        updated.StatusName.Should().Be("InProgress");

        await repository.DeleteAsync(created.Id, DemoUserId);
    }

    [SkippableFact]
    public async Task ListForUserAsync_ReturnsOnlyOwnedTasks()
    {
        Skip.IfNot(await PostgresTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");
        await using var context = PostgresTestDatabase.CreateContext();
        var repository = new EfTaskRepository(context, NullLogger<EfTaskRepository>.Instance);

        var tasks = await repository.ListForUserAsync(DemoUserId);

        tasks.Should().NotBeEmpty();
        tasks.Should().OnlyContain(t => t.UserId == DemoUserId);
    }
}
