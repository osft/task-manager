using TaskManager.Application.Auth;
using TaskManager.Application.Auth.Models;
using TaskManager.Application.Users;

namespace TaskManager.Infrastructure.Auth;

public sealed class LocalCredentialsAuthenticationProvider : IAuthenticationProvider
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LocalCredentialsAuthenticationProvider(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public string ProviderName => "local";

    public async Task<AuthenticatedUser?> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = await _userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return null;
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        return new AuthenticatedUser
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Alias = user.Alias,
            RoleName = user.RoleName,
            MfaEnabled = user.MfaEnabled,
            IsActive = user.IsActive,
            EndDate = user.EndDate,
            AuthProvider = ProviderName
        };
    }
}
