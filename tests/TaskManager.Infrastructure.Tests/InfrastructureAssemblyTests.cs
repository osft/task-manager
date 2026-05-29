using FluentAssertions;
using Microsoft.Extensions.Configuration;
using TaskManager.Infrastructure;

namespace TaskManager.Infrastructure.Tests;

public class InfrastructureAssemblyTests
{
    [Fact]
    public void DependencyInjection_ShouldRegisterWithoutError()
    {
        var configuration = Persistence.PostgresTestDatabase.CreateConfiguration();

        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        var result = services.AddInfrastructure(configuration);
        result.Should().BeSameAs(services);
    }
}
