namespace TaskManager.Api.Contracts.Users;

public sealed class UserProfileResponse
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string Name { get; init; }

    public string? Alias { get; init; }

    public required string RoleName { get; init; }

    public bool IsActive { get; init; }

    public DateTime? LastLoginDate { get; init; }
}
