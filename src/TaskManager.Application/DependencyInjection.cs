using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application.Auth;
using TaskManager.Application.Tasks;
using TaskManager.Application.Users;
using TaskManager.Domain;

namespace TaskManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddDomain();

        services.AddScoped<RegisterUserUseCase>();
        services.AddScoped<LoginUserUseCase>();
        services.AddScoped<GetUserProfileUseCase>();

        services.AddScoped<CreateTaskUseCase>();
        services.AddScoped<GetTaskUseCase>();
        services.AddScoped<ListTasksUseCase>();
        services.AddScoped<UpdateTaskUseCase>();
        services.AddScoped<DeleteTaskUseCase>();
        services.AddScoped<ListTaskStatusesUseCase>();

        return services;
    }
}
