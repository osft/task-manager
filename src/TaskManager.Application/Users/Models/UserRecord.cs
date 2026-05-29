namespace TaskManager.Application.Users.Models;

public sealed class UserRecord
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string Name { get; init; }

    public string? Alias { get; init; }

    public string? PasswordHash { get; init; }

    public required int RoleId { get; init; }

    public required string RoleName { get; init; }

    public bool IsActive { get; init; }

    public DateTime? EndDate { get; init; }

    public DateTime? LastLoginDate { get; init; }

    public bool MfaEnabled { get; init; }
}
