namespace TaskManager.Application.Auth.Commands;

public sealed class LoginUserCommand
{
    public required string Email { get; init; }

    public required string Password { get; init; }

    public string Provider { get; init; } = "local";
}
