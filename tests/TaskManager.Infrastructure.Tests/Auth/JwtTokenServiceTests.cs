using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using TaskManager.Application.Auth.Models;
using TaskManager.Infrastructure.Auth;

namespace TaskManager.Infrastructure.Tests.Auth;

public class JwtTokenServiceTests
{
    [Fact]
    public void CreateToken_IncludesRequiredClaims()
    {
        var settings = Options.Create(new JwtSettings
        {
            Secret = "super-secret-key-at-least-32-characters-long",
            Issuer = "TaskManager",
            Audience = "TaskManager",
            ExpirationMinutes = 60
        });

        var sut = new JwtTokenService(settings);
        var user = new AuthenticatedUser
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Email = "demo@taskmanager.local",
            Name = "Demo PM",
            Alias = "demo-pm",
            RoleName = "ProjectManager",
            MfaEnabled = false,
            IsActive = true,
            AuthProvider = "local"
        };

        var result = sut.CreateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == user.Email);
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.Name);
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "ProjectManager");
        jwt.Claims.Should().Contain(c => c.Type == "auth_provider" && c.Value == "local");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }
}
