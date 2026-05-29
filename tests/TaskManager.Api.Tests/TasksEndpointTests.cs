using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using TaskManager.Api.Contracts.Tasks;
using TaskManager.Api.Tests.Support;
using TaskManager.Domain.Enums;

namespace TaskManager.Api.Tests;

[Trait("Category", "Integration")]
public class TasksEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly WebApplicationFactory<Program> _factory;

    public TasksEndpointTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ListTasks_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tasks");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [SkippableFact]
    public async Task CreateTask_WithValidStandardPriority_ReturnsCreated()
    {
        Skip.IfNot(await ApiTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");

        var client = _factory.CreateClient();
        var token = await ApiTestAuth.LoginDemoUserAsync(client);
        ApiTestAuth.SetBearerToken(client, token);

        var dueDate = DateTime.UtcNow.AddMonths(1);
        var response = await client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest
        {
            Title = $"API test task {Guid.NewGuid():N}",
            Description = "Created from integration test",
            Priority = TaskPriority.Standard,
            DueDate = dueDate
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var task = await response.Content.ReadFromJsonAsync<TaskResponse>(JsonOptions);
        task.Should().NotBeNull();
        task!.Priority.Should().Be(TaskPriority.Standard);
        task.StatusName.Should().Be("Todo");

        await client.DeleteAsync($"/api/tasks/{task.Id}");
    }

    [SkippableFact]
    public async Task CreateTask_WithHighPriorityBeyond48Hours_ReturnsBadRequest()
    {
        Skip.IfNot(await ApiTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");

        var client = _factory.CreateClient();
        var token = await ApiTestAuth.LoginDemoUserAsync(client);
        ApiTestAuth.SetBearerToken(client, token);

        var response = await client.PostAsJsonAsync("/api/tasks", new CreateTaskRequest
        {
            Title = "Invalid high priority task",
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddHours(49)
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [SkippableFact]
    public async Task GetTaskStatuses_WithToken_ReturnsSeededStatuses()
    {
        Skip.IfNot(await ApiTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");

        var client = _factory.CreateClient();
        var token = await ApiTestAuth.LoginDemoUserAsync(client);
        ApiTestAuth.SetBearerToken(client, token);

        var response = await client.GetAsync("/api/task-statuses");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var statuses = await response.Content.ReadFromJsonAsync<List<TaskStatusResponse>>(JsonOptions);
        statuses.Should().NotBeNull();
        statuses!.Select(s => s.Name).Should().Contain(new[] { "Todo", "InProgress", "Done" });
    }

    [SkippableFact]
    public async Task GetTask_WhenTaskDoesNotExist_ReturnsNotFound()
    {
        Skip.IfNot(await ApiTestDatabase.IsAvailableAsync(), "PostgreSQL not available (docker compose up postgres -d).");

        var client = _factory.CreateClient();
        var token = await ApiTestAuth.LoginDemoUserAsync(client);
        ApiTestAuth.SetBearerToken(client, token);

        var response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
