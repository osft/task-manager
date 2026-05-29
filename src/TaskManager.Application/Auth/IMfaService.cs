using TaskManager.Application.Auth.Models;

namespace TaskManager.Application.Auth;

public interface IMfaService
{
    Task<bool> IsMfaRequiredAsync(AuthenticatedUser user, CancellationToken cancellationToken = default);

    Task<string> BeginMfaChallengeAsync(AuthenticatedUser user, CancellationToken cancellationToken = default);
}
