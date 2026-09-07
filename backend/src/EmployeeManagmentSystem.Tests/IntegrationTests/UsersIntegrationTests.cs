using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;

namespace EmployeeManagmentSystem.Tests.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class UsersIntegrationTests(EmployeeManagementApiFactory factory) : IAsyncLifetime
{
    private HttpClient client = null!;
    private ApiTestClient api = null!;

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
        client = factory.CreateClient();
        api = new ApiTestClient(client, factory);
        await api.AuthenticateAsAdminAsync();
    }

    public Task DisposeAsync()
    {
        client.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task UsersEndpoints_ShouldCreateListFilterGetAndUpdateUsers()
    {
        var activeId = await api.CreateUserAsync("USR-001", "active.user");
        var inactiveId = await api.CreateUserAsync("USR-002", "inactive.user", status: EntityStatus.Inactive);

        var allUsers = await client.GetFromJsonAsync<IReadOnlyCollection<UserDto>>("/api/v1/users");
        var inactiveUsers = await client.GetFromJsonAsync<IReadOnlyCollection<UserDto>>("/api/v1/users?status=Inactive");
        var activeUser = await client.GetFromJsonAsync<UserDto>($"/api/v1/users/{activeId}");
        var updateResponse = await client.PatchAsJsonAsync(
            $"/api/v1/users/{activeId}",
            new UpdateUserRequest("NewPassword123!", EntityStatus.Inactive));
        var loginWithNewPassword = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest("active.user", "NewPassword123!"));

        Assert.NotNull(allUsers);
        Assert.Contains(allUsers, user => user.Id == activeId);
        Assert.Contains(allUsers, user => user.Id == inactiveId);
        Assert.NotNull(inactiveUsers);
        Assert.Contains(inactiveUsers, user => user.Id == inactiveId);
        Assert.DoesNotContain(inactiveUsers, user => user.Id == activeId);
        Assert.NotNull(activeUser);
        Assert.Equal("active.user", activeUser.Login);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, loginWithNewPassword.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateCodeOrLogin_ShouldReturnConflict()
    {
        await api.CreateUserAsync("USR-001", "duplicate.user");

        var duplicateCode = await client.PostAsJsonAsync(
            "/api/v1/users",
            new CreateUserRequest("USR-001", "another.user", "Password123!", EntityStatus.Active));
        var duplicateLogin = await client.PostAsJsonAsync(
            "/api/v1/users",
            new CreateUserRequest("USR-002", "duplicate.user", "Password123!", EntityStatus.Active));

        Assert.Equal(HttpStatusCode.Conflict, duplicateCode.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, duplicateLogin.StatusCode);
    }

    [RabbitMqFact]
    public async Task CreateUser_ShouldPublishUserRegisteredEventToRabbitMq()
    {
        var connectionFactory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RabbitMq__Host") ?? "localhost",
            Port = int.TryParse(Environment.GetEnvironmentVariable("RabbitMq__Port"), out var port) ? port : 5672,
            UserName = Environment.GetEnvironmentVariable("RabbitMq__UserName") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RabbitMq__Password") ?? "guest"
        };

        using var connection = connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();
        var exchange = Environment.GetEnvironmentVariable("RabbitMq__Exchange") ?? "employee-management.events.v2";
        var unroutedExchange = Environment.GetEnvironmentVariable("RabbitMq__UnroutedExchange") ?? "employee-management.unrouted.v2";
        channel.ExchangeDeclare(unroutedExchange, ExchangeType.Fanout, durable: true, autoDelete: false);
        channel.ExchangeDeclare(
            exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: new Dictionary<string, object> { ["alternate-exchange"] = unroutedExchange });
        var queue = channel.QueueDeclare().QueueName;
        channel.QueueBind(queue, exchange, "UserRegisteredEvent");
        var userId = await api.CreateUserAsync($"RABBIT-{Guid.NewGuid():N}", $"rabbit-{Guid.NewGuid():N}");
        var deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            var message = channel.BasicGet(queue, autoAck: true);
            if (message is not null)
            {
                var payload = Encoding.UTF8.GetString(message.Body.ToArray());
                var registeredEvent = JsonSerializer.Deserialize<UserRegisteredEventPayload>(payload);
                if (registeredEvent?.UserId == userId)
                {
                    Assert.Equal("UserRegisteredEvent", message.RoutingKey);
                    return;
                }
            }

            await Task.Delay(100);
        }

        Assert.Fail($"UserRegisteredEvent for user {userId} was not found in queue {queue}.");
    }

    [Fact]
    public async Task MissingOrInvalidUserRequests_ShouldReturnExpectedProblemDetails()
    {
        var missingId = Guid.NewGuid();

        var notFound = await client.GetAsync($"/api/v1/users/{missingId}");
        var invalid = await client.PostAsJsonAsync(
            "/api/v1/users",
            new CreateUserRequest(string.Empty, string.Empty, string.Empty, EntityStatus.Active));

        var notFoundProblem = await notFound.Content.ReadFromJsonAsync<ProblemDetails>();
        var validationProblem = await invalid.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
        Assert.NotNull(notFoundProblem);
        Assert.Equal("Resource not found", notFoundProblem.Title);
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.NotNull(validationProblem);
        Assert.Equal("Validation failed", validationProblem.Title);
        Assert.NotEmpty(validationProblem.Errors);
    }

    private sealed record UserRegisteredEventPayload(Guid UserId, string Code, string Login, DateTimeOffset OccurredAt);
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class RabbitMqFactAttribute : FactAttribute
{
    public RabbitMqFactAttribute()
    {
        if (!bool.TryParse(Environment.GetEnvironmentVariable("RabbitMq__Enabled"), out var enabled) || !enabled)
        {
            Skip = "RabbitMQ integration tests require RabbitMq__Enabled=true.";
        }
    }
}
