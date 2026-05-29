using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Contracts.Users;
using TaskManager.Api.Extensions;
using TaskManager.Application.Users;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public sealed class MeController : ControllerBase
{
    private readonly GetUserProfileUseCase _getUserProfileUseCase;

    public MeController(GetUserProfileUseCase getUserProfileUseCase)
    {
        _getUserProfileUseCase = getUserProfileUseCase;
    }

    /// <summary>Returns the authenticated user's profile.</summary>
    /// <returns>Current user profile.</returns>
    /// <response code="200">Profile returned.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> GetProfile(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var profile = await _getUserProfileUseCase.ExecuteAsync(userId, cancellationToken);

        return Ok(new UserProfileResponse
        {
            Id = profile.Id,
            Email = profile.Email,
            Name = profile.Name,
            Alias = profile.Alias,
            RoleName = profile.RoleName,
            IsActive = profile.IsActive,
            LastLoginDate = profile.LastLoginDate
        });
    }
}
