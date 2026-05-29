namespace TaskManager.Application.Auth.Commands;

public sealed class RegisterUserCommand
{
    public required string Name { get; init; }

    public string? Alias { get; init; }

    public required string Email { get; init; }

    public required string Password { get; init; }
}
