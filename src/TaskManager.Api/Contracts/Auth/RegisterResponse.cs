namespace TaskManager.Api.Contracts.Auth;

public sealed class RegisterResponse
{
    public required Guid UserId { get; init; }
}
