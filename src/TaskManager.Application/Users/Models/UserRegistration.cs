namespace TaskManager.Application.Users.Models;

public sealed class UserRegistration
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string Name { get; init; }

    public string? Alias { get; init; }

    public required string PasswordHash { get; init; }

    public required int RoleId { get; init; }

    public required Guid CreatedBy { get; init; }

    public required DateTime CreatedOnUtc { get; init; }
}
