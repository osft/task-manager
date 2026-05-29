using FluentAssertions;
using TaskManager.Application.Auth;
using TaskManager.Infrastructure.Auth;

namespace TaskManager.Infrastructure.Tests.Auth;

public class AuthenticationProviderResolverTests
{
    [Fact]
    public void Resolve_WhenLocal_ReturnsLocalProvider()
    {
        IAuthenticationProvider local = new LocalCredentialsAuthenticationProvider(
            new Moq.Mock<Application.Users.IUserRepository>().Object,
            new Moq.Mock<Application.Auth.IPasswordHasher>().Object);

        var sut = new AuthenticationProviderResolver(new[] { local });

        sut.Resolve("local").ProviderName.Should().Be("local");
    }

    [Fact]
    public void Resolve_WhenUnknownProvider_Throws()
    {
        IAuthenticationProvider local = new LocalCredentialsAuthenticationProvider(
            new Moq.Mock<Application.Users.IUserRepository>().Object,
            new Moq.Mock<Application.Auth.IPasswordHasher>().Object);

        var sut = new AuthenticationProviderResolver(new[] { local });

        var act = () => sut.Resolve("google");

        act.Should().Throw<ArgumentException>();
    }
}
