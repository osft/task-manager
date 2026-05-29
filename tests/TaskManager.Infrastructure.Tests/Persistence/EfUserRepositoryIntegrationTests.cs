using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using TaskManager.Application.Users.Models;
using TaskManager.Infrastructure.Persistence.Repositories;

namespace TaskManager.Infrastructure.Tests.Persistence;

[Trait("Category", "Integration")]
public class EfUserRepositoryIntegrationTests
{
    [SkippableFact]
    public async Task GetByEmailAsync_ReturnsDemoUserWithRole()
    {
        Skip.IfNot(await PostgresTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");
        await using var context = PostgresTestDatabase.CreateContext();
        var repository = new EfUserRepository(context, NullLogger<EfUserRepository>.Instance);

        var user = await repository.GetByEmailAsync("demo@taskmanager.local");

        user.Should().NotBeNull();
        user!.RoleName.Should().Be("ProjectManager");
        user.IsActive.Should().BeTrue();
    }

    [SkippableFact]
    public async Task ExistsByEmailAsync_IsCaseInsensitive()
    {
        Skip.IfNot(await PostgresTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");
        await using var context = PostgresTestDatabase.CreateContext();
        var repository = new EfUserRepository(context, NullLogger<EfUserRepository>.Instance);

        var exists = await repository.ExistsByEmailAsync("DEMO@taskmanager.local");

        exists.Should().BeTrue();
    }
}
