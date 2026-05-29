namespace TaskManager.Application.Auth.Models;

public sealed class AuthTokenResult
{
    public required string Token { get; init; }

    public required DateTime ExpiresAt { get; init; }
}
