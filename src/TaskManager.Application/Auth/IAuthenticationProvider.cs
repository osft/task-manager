using TaskManager.Application.Auth.Models;

namespace TaskManager.Application.Auth;

public interface IAuthenticationProvider
{
    string ProviderName { get; }

    Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);
}
