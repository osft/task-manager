namespace TaskManager.Application.Users.Models;

public sealed class UserProfile
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string Name { get; init; }

    public string? Alias { get; init; }

    public required string RoleName { get; init; }

    public bool IsActive { get; init; }

    public DateTime? LastLoginDate { get; init; }
}
