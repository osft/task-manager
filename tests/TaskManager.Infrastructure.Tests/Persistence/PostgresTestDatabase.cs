using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Infrastructure.Tests.Persistence;

internal static class PostgresTestDatabase
{
    private const string DefaultConnection =
        "Host=localhost;Port=5433;Database=taskmanager;Username=taskmanager;Password=taskmanager_dev";

    internal static string ConnectionString =>
        Environment.GetEnvironmentVariable("TASK_MANAGER_TEST_CONNECTION")
        ?? DefaultConnection;

    internal static async Task<bool> IsAvailableAsync()
    {
        try
        {
            await using var context = new TaskManagerDbContext(CreateOptions());
            return await context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }

    internal static TaskManagerDbContext CreateContext() =>
        new(CreateOptions());

    internal static DbContextOptions<TaskManagerDbContext> CreateOptions() =>
        new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

    internal static IConfiguration CreateConfiguration() =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = ConnectionString,
                ["Jwt:Secret"] = "super-secret-key-at-least-32-characters-long",
                ["Jwt:Issuer"] = "TaskManager",
                ["Jwt:Audience"] = "TaskManager",
                ["Jwt:ExpirationMinutes"] = "60"
            })
            .Build();
}
