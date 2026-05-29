using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Application;
using TaskManager.Application.Auth;
using TaskManager.Application.Tasks;
using TaskManager.Application.Users;
using TaskManager.Infrastructure.Auth;
using TaskManager.Infrastructure.Persistence;
using TaskManager.Infrastructure.Persistence.Repositories;

namespace TaskManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddApplication();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<TaskManagerDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITaskRepository, EfTaskRepository>();
        services.AddScoped<IUserRepository, EfUserRepository>();
        services.AddScoped<IRoleRepository, EfRoleRepository>();
        services.AddScoped<ITaskStatusRepository, EfTaskStatusRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IMfaService, NoOpMfaService>();

        services.AddScoped<IAuthenticationProvider, LocalCredentialsAuthenticationProvider>();
        services.AddScoped<IAuthenticationProviderResolver, AuthenticationProviderResolver>();

        return services;
    }
}
