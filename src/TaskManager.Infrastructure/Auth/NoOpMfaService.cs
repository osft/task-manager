using TaskManager.Application.Auth;
using TaskManager.Application.Auth.Models;

namespace TaskManager.Infrastructure.Auth;

public sealed class NoOpMfaService : IMfaService
{
    public Task<bool> IsMfaRequiredAsync(AuthenticatedUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult(false);

    public Task<string> BeginMfaChallengeAsync(AuthenticatedUser user, CancellationToken cancellationToken = default) =>
        Task.FromResult(Guid.NewGuid().ToString("N"));
}
