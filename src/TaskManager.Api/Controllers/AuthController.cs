using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Contracts.Auth;
using TaskManager.Application.Auth;
using TaskManager.Application.Auth.Commands;
using TaskManager.Application.Exceptions;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUserUseCase;
    private readonly LoginUserUseCase _loginUserUseCase;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        RegisterUserUseCase registerUserUseCase,
        LoginUserUseCase loginUserUseCase,
        ILogger<AuthController> logger)
    {
        _registerUserUseCase = registerUserUseCase;
        _loginUserUseCase = loginUserUseCase;
        _logger = logger;
    }

    /// <summary>Registers a new local user account.</summary>
    /// <param name="request">Registration details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created user id.</returns>
    /// <response code="201">User created.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="409">Email already exists.</response>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = await _registerUserUseCase.ExecuteAsync(
                new RegisterUserCommand
                {
                    Name = request.Name,
                    Alias = request.Alias,
                    Email = request.Email,
                    Password = request.Password
                },
                cancellationToken);

            return CreatedAtAction(nameof(Register), new RegisterResponse { UserId = userId });
        }
        catch (ConflictException ex)
        {
            _logger.LogWarning(
                ex,
                "Registration failed for {Email} from {ClientIp}",
                request.Email,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            throw;
        }
    }

    /// <summary>Authenticates a user and returns a JWT (or MFA challenge when enabled).</summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>JWT token or MFA challenge metadata.</returns>
    /// <response code="200">Authenticated successfully.</response>
    /// <response code="401">Invalid credentials.</response>
    /// <response code="403">Account inactive or expired.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _loginUserUseCase.ExecuteAsync(
                new LoginUserCommand
                {
                    Email = request.Email,
                    Password = request.Password,
                    Provider = request.Provider
                },
                cancellationToken);

            return Ok(new AuthResponse
            {
                Token = result.Token?.Token,
                ExpiresAt = result.Token?.ExpiresAt,
                RequiresMfa = result.RequiresMfa,
                MfaSessionId = result.MfaSessionId
            });
        }
        catch (UnauthorizedApplicationException ex)
        {
            _logger.LogWarning(
                ex,
                "Login failed for {Email} from {ClientIp}",
                request.Email,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            throw;
        }
        catch (ForbiddenApplicationException ex)
        {
            _logger.LogWarning(
                ex,
                "Login forbidden for {Email} from {ClientIp}",
                request.Email,
                HttpContext.Connection.RemoteIpAddress?.ToString());
            throw;
        }
    }
}
