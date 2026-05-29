using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Contracts.Tasks;
using TaskManager.Api.Extensions;
using TaskManager.Application.Tasks;
using TaskManager.Application.Tasks.Commands;
using TaskManager.Domain.Exceptions;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly CreateTaskUseCase _createTaskUseCase;
    private readonly GetTaskUseCase _getTaskUseCase;
    private readonly ListTasksUseCase _listTasksUseCase;
    private readonly UpdateTaskUseCase _updateTaskUseCase;
    private readonly DeleteTaskUseCase _deleteTaskUseCase;
    private readonly ILogger<TasksController> _logger;

    public TasksController(
        CreateTaskUseCase createTaskUseCase,
        GetTaskUseCase getTaskUseCase,
        ListTasksUseCase listTasksUseCase,
        UpdateTaskUseCase updateTaskUseCase,
        DeleteTaskUseCase deleteTaskUseCase,
        ILogger<TasksController> logger)
    {
        _createTaskUseCase = createTaskUseCase;
        _getTaskUseCase = getTaskUseCase;
        _listTasksUseCase = listTasksUseCase;
        _updateTaskUseCase = updateTaskUseCase;
        _deleteTaskUseCase = deleteTaskUseCase;
        _logger = logger;
    }

    /// <summary>Lists tasks for the authenticated user.</summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Tasks owned by the current user.</returns>
    /// <response code="200">Task list returned.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TaskResponse>>> List(CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var tasks = await _listTasksUseCase.ExecuteAsync(userId, cancellationToken);
        return Ok(tasks.Select(t => t.ToResponse()).ToList());
    }

    /// <summary>Returns a single task owned by the authenticated user.</summary>
    /// <param name="id">Task unique identifier from the route.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching task.</returns>
    /// <response code="200">Task found and returned.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    /// <response code="404">Task not found or not owned by the current user.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> GetById(
        [FromRoute][Required] Guid id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        var task = await _getTaskUseCase.ExecuteAsync(id, userId, cancellationToken);
        return Ok(task.ToResponse());
    }

    /// <summary>Creates a new task for the authenticated user.</summary>
    /// <param name="request">Task creation payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created task.</returns>
    /// <response code="201">Task created.</response>
    /// <response code="400">Validation error (DTO or domain rules).</response>
    /// <response code="401">Missing or invalid JWT.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TaskResponse>> Create(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var task = await _createTaskUseCase.ExecuteAsync(
                new CreateTaskCommand
                {
                    UserId = userId,
                    Title = request.Title,
                    Description = request.Description,
                    Priority = request.Priority,
                    DueDate = request.DueDate
                },
                cancellationToken);

            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task.ToResponse());
        }
        catch (DomainValidationException ex)
        {
            _logger.LogInformation(
                ex,
                "Task create rejected for user {UserId} with priority {Priority} and due date {DueDate}",
                userId,
                request.Priority,
                request.DueDate);
            throw;
        }
    }

    /// <summary>Updates an existing task owned by the authenticated user.</summary>
    /// <param name="id">Task unique identifier from the route.</param>
    /// <param name="request">Task update payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    /// <response code="200">Task updated.</response>
    /// <response code="400">Validation error (DTO or domain rules).</response>
    /// <response code="401">Missing or invalid JWT.</response>
    /// <response code="404">Task not found or not owned by the current user.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Update(
        [FromRoute][Required] Guid id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var task = await _updateTaskUseCase.ExecuteAsync(
                new UpdateTaskCommand
                {
                    TaskId = id,
                    UserId = userId,
                    Title = request.Title,
                    Description = request.Description,
                    Priority = request.Priority,
                    DueDate = request.DueDate,
                    StatusId = request.StatusId
                },
                cancellationToken);

            return Ok(task.ToResponse());
        }
        catch (DomainValidationException ex)
        {
            _logger.LogInformation(
                ex,
                "Task update rejected for user {UserId}, task {TaskId}, priority {Priority}, due date {DueDate}",
                userId,
                id,
                request.Priority,
                request.DueDate);
            throw;
        }
    }

    /// <summary>Deletes a task owned by the authenticated user.</summary>
    /// <param name="id">Task unique identifier from the route.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="204">Task deleted.</response>
    /// <response code="401">Missing or invalid JWT.</response>
    /// <response code="404">Task not found or not owned by the current user.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute][Required] Guid id,
        CancellationToken cancellationToken)
    {
        if (!User.TryGetUserId(out var userId))
        {
            return Unauthorized();
        }

        await _deleteTaskUseCase.ExecuteAsync(
            new DeleteTaskCommand { TaskId = id, UserId = userId },
            cancellationToken);

        return NoContent();
    }
}
