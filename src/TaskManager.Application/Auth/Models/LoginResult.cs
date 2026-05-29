namespace TaskManager.Application.Auth.Models;

public sealed class LoginResult
{
    public bool RequiresMfa { get; init; }

    public string? MfaSessionId { get; init; }

    public AuthTokenResult? Token { get; init; }

    public static LoginResult MfaRequired(string mfaSessionId) =>
        new() { RequiresMfa = true, MfaSessionId = mfaSessionId };

    public static LoginResult Success(AuthTokenResult token) =>
        new() { Token = token };
}
