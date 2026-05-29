using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Contracts.Tasks;
using TaskManager.Api.Extensions;
using TaskManager.Application.Tasks;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/task-statuses")]
[Authorize]
public sealed class TaskStatusesController : ControllerBase
{
    private readonly ListTaskStatusesUseCase _listTaskStatusesUseCase;

    public TaskStatusesController(ListTaskStatusesUseCase listTaskStatusesUseCase)
    {
        _listTaskStatusesUseCase = listTaskStatusesUseCase;
    }

    /// <summary>Returns active task statuses for UI dropdowns.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Active task statuses ordered by sort order.</returns>
    /// <response code="200">Statuses returned.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TaskStatusResponse>>> List(CancellationToken cancellationToken)
    {
        var statuses = await _listTaskStatusesUseCase.ExecuteAsync(cancellationToken);
        return Ok(statuses.Select(s => s.ToResponse()).ToList());
    }
}
