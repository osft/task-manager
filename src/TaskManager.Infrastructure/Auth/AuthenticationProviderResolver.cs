using TaskManager.Application.Auth;

namespace TaskManager.Infrastructure.Auth;

public sealed class AuthenticationProviderResolver : IAuthenticationProviderResolver
{
    private readonly IReadOnlyDictionary<string, IAuthenticationProvider> _providers;

    public AuthenticationProviderResolver(IEnumerable<IAuthenticationProvider> providers)
    {
        _providers = providers.ToDictionary(
            p => p.ProviderName,
            StringComparer.OrdinalIgnoreCase);
    }

    public IAuthenticationProvider Resolve(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = "local";
        }

        if (_providers.TryGetValue(provider, out var authenticationProvider))
        {
            return authenticationProvider;
        }

        throw new ArgumentException($"Authentication provider '{provider}' is not supported.", nameof(provider));
    }
}
