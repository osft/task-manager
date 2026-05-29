using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Domain;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Factories;
using TaskManager.Domain.Strategies;

namespace TaskManager.Domain.Tests.DependencyInjection;

public class DomainDependencyInjectionTests
{
    [Fact]
    public void AddDomain_RegistersStrategyAndFactoryServices()
    {
        var services = new ServiceCollection();
        services.AddDomain();

        using var provider = services.BuildServiceProvider();

        provider.GetService<ITaskValidationStrategyResolver>().Should().NotBeNull();
        provider.GetService<ITaskDueDateValidator>().Should().NotBeNull();
        provider.GetService<ITaskFactory>().Should().NotBeNull();
    }

    [Fact]
    public void AddDomain_RegisteredFactoryCreatesValidTask()
    {
        var services = new ServiceCollection();
        services.AddDomain();

        using var provider = services.BuildServiceProvider();
        var factory = provider.GetRequiredService<ITaskFactory>();

        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var task = factory.Create(
            userId,
            "DI wired task",
            "desc",
            TaskPriority.Standard,
            DateTime.UtcNow.AddDays(7),
            DateTime.UtcNow);

        task.Title.Should().Be("DI wired task");
    }
}
