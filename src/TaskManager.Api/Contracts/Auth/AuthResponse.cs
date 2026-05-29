namespace TaskManager.Api.Contracts.Auth;

public sealed class AuthResponse
{
    public string? Token { get; init; }

    public DateTime? ExpiresAt { get; init; }

    public bool RequiresMfa { get; init; }

    public string? MfaSessionId { get; init; }
}
