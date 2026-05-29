using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Infrastructure.Persistence.Repositories;

namespace TaskManager.Infrastructure.Tests.Persistence;

[Trait("Category", "Integration")]
public class EfTaskStatusRepositoryIntegrationTests
{
    [SkippableFact]
    public async Task GetAllActiveAsync_ReturnsSeededStatuses()
    {
        Skip.IfNot(await PostgresTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");
        await using var context = PostgresTestDatabase.CreateContext();
        var repository = new EfTaskStatusRepository(context, NullLogger<EfTaskStatusRepository>.Instance);

        var statuses = await repository.GetAllActiveAsync();

        statuses.Should().HaveCountGreaterThanOrEqualTo(3);
        statuses.Select(s => s.Name).Should().Contain(new[] { "Todo", "InProgress", "Done" });
    }
}
