using TaskManager.Application.Auth.Commands;
using TaskManager.Application.Auth.Models;
using TaskManager.Application.Exceptions;

namespace TaskManager.Application.Auth;

public sealed class LoginUserUseCase
{
    private readonly IAuthenticationProviderResolver _providerResolver;
    private readonly IMfaService _mfaService;
    private readonly ITokenService _tokenService;
    private readonly Users.IUserRepository _userRepository;

    public LoginUserUseCase(
        IAuthenticationProviderResolver providerResolver,
        IMfaService mfaService,
        ITokenService tokenService,
        Users.IUserRepository userRepository)
    {
        _providerResolver = providerResolver;
        _mfaService = mfaService;
        _tokenService = tokenService;
        _userRepository = userRepository;
    }

    public async Task<LoginResult> ExecuteAsync(LoginUserCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var provider = _providerResolver.Resolve(command.Provider);
        var user = await provider.AuthenticateAsync(command.Email, command.Password, cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedApplicationException();
        }

        if (!user.IsActive)
        {
            throw new ForbiddenApplicationException("User account is inactive.");
        }

        if (user.EndDate.HasValue && user.EndDate.Value < DateTime.UtcNow)
        {
            throw new ForbiddenApplicationException("User account has expired.");
        }

        if (await _mfaService.IsMfaRequiredAsync(user, cancellationToken))
        {
            var sessionId = await _mfaService.BeginMfaChallengeAsync(user, cancellationToken);
            return LoginResult.MfaRequired(sessionId);
        }

        var token = _tokenService.CreateToken(user);
        await _userRepository.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        return LoginResult.Success(token);
    }
}
