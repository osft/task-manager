namespace TaskManager.Application.Auth.Models;

public sealed class AuthenticatedUser
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string Name { get; init; }

    public string? Alias { get; init; }

    public required string RoleName { get; init; }

    public bool MfaEnabled { get; init; }

    public bool IsActive { get; init; }

    public DateTime? EndDate { get; init; }

    public string AuthProvider { get; init; } = "local";
}
