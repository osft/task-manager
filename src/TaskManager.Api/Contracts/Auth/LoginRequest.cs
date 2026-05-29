using System.ComponentModel.DataAnnotations;

namespace TaskManager.Api.Contracts.Auth;

public sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    public string Provider { get; init; } = "local";
}
