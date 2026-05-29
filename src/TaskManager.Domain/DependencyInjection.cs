using Microsoft.Extensions.DependencyInjection;
using TaskManager.Domain.Factories;
using TaskManager.Domain.Strategies;

namespace TaskManager.Domain;

public static class DependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddSingleton<ITaskValidationStrategyResolver, TaskValidationStrategyResolver>();
        services.AddSingleton<ITaskDueDateValidator, TaskDueDateValidator>();
        services.AddSingleton<ITaskFactory, Factories.TaskFactory>();

        return services;
    }
}
