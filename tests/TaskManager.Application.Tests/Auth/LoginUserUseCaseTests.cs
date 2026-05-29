using FluentAssertions;
using Moq;
using TaskManager.Application.Auth;
using TaskManager.Application.Auth.Commands;
using TaskManager.Application.Auth.Models;
using TaskManager.Application.Exceptions;
using TaskManager.Application.Users;

namespace TaskManager.Application.Tests.Auth;

public class LoginUserUseCaseTests
{
    private readonly Mock<IAuthenticationProviderResolver> _providerResolver = new();
    private readonly Mock<IAuthenticationProvider> _localProvider = new();
    private readonly Mock<IMfaService> _mfaService = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly LoginUserUseCase _sut;

    private static readonly AuthenticatedUser ActiveUser = new()
    {
        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
        Email = "demo@taskmanager.local",
        Name = "Demo PM",
        RoleName = "ProjectManager",
        MfaEnabled = false,
        IsActive = true,
        EndDate = null,
        AuthProvider = "local"
    };

    public LoginUserUseCaseTests()
    {
        _localProvider.Setup(p => p.ProviderName).Returns("local");
        _providerResolver.Setup(r => r.Resolve("local")).Returns(_localProvider.Object);

        _mfaService.Setup(m => m.IsMfaRequiredAsync(It.IsAny<AuthenticatedUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _tokenService.Setup(t => t.CreateToken(It.IsAny<AuthenticatedUser>()))
            .Returns(new AuthTokenResult { Token = "jwt-token", ExpiresAt = DateTime.UtcNow.AddHours(1) });

        _sut = new LoginUserUseCase(
            _providerResolver.Object,
            _mfaService.Object,
            _tokenService.Object,
            _userRepository.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCredentialsAreValid_ReturnsTokenAndUpdatesLastLogin()
    {
        _localProvider.Setup(p => p.AuthenticateAsync("demo@taskmanager.local", "Password123!", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ActiveUser);

        var result = await _sut.ExecuteAsync(new LoginUserCommand
        {
            Email = "demo@taskmanager.local",
            Password = "Password123!"
        });

        result.RequiresMfa.Should().BeFalse();
        result.Token!.Token.Should().Be("jwt-token");
        _userRepository.Verify(
            r => r.UpdateLastLoginAsync(ActiveUser.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCredentialsAreInvalid_ThrowsUnauthorized()
    {
        _localProvider.Setup(p => p.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthenticatedUser?)null);

        var act = () => _sut.ExecuteAsync(new LoginUserCommand
        {
            Email = "demo@taskmanager.local",
            Password = "wrong"
        });

        await act.Should().ThrowAsync<UnauthorizedApplicationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserIsInactive_ThrowsForbidden()
    {
        _localProvider.Setup(p => p.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser(isActive: false));

        var act = () => _sut.ExecuteAsync(new LoginUserCommand
        {
            Email = "demo@taskmanager.local",
            Password = "Password123!"
        });

        await act.Should().ThrowAsync<ForbiddenApplicationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenAccountExpired_ThrowsForbidden()
    {
        _localProvider.Setup(p => p.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser(endDate: DateTime.UtcNow.AddDays(-1)));

        var act = () => _sut.ExecuteAsync(new LoginUserCommand
        {
            Email = "demo@taskmanager.local",
            Password = "Password123!"
        });

        await act.Should().ThrowAsync<ForbiddenApplicationException>();
    }

    [Fact]
    public async Task ExecuteAsync_WhenMfaRequired_ReturnsMfaChallengeWithoutToken()
    {
        _localProvider.Setup(p => p.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateUser(mfaEnabled: true));

        _mfaService.Setup(m => m.IsMfaRequiredAsync(It.IsAny<AuthenticatedUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mfaService.Setup(m => m.BeginMfaChallengeAsync(It.IsAny<AuthenticatedUser>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("mfa-session-123");

        var result = await _sut.ExecuteAsync(new LoginUserCommand
        {
            Email = "demo@taskmanager.local",
            Password = "Password123!"
        });

        result.RequiresMfa.Should().BeTrue();
        result.MfaSessionId.Should().Be("mfa-session-123");
        result.Token.Should().BeNull();
        _userRepository.Verify(
            r => r.UpdateLastLoginAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static AuthenticatedUser CreateUser(
        bool isActive = true,
        DateTime? endDate = null,
        bool mfaEnabled = false) =>
        new()
        {
            Id = ActiveUser.Id,
            Email = ActiveUser.Email,
            Name = ActiveUser.Name,
            RoleName = ActiveUser.RoleName,
            MfaEnabled = mfaEnabled,
            IsActive = isActive,
            EndDate = endDate,
            AuthProvider = "local"
        };
}
