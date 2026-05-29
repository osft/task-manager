using Microsoft.EntityFrameworkCore;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api.Tests.Support;

internal static class ApiTestDatabase
{
    private const string DefaultConnection =
        "Host=localhost;Port=5433;Database=taskmanager;Username=taskmanager;Password=taskmanager_dev";

    internal static async Task<bool> IsAvailableAsync()
    {
        try
        {
            var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
                .UseNpgsql(DefaultConnection)
                .Options;

            await using var context = new TaskManagerDbContext(options);
            return await context.Database.CanConnectAsync();
        }
        catch
        {
            return false;
        }
    }
}
