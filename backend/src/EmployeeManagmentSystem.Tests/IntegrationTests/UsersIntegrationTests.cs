using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Http.Json;
using EmployeeManagmentSystem.Application.Common.Events;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;
using EmployeeManagmentSystem.Infrastructure.Services.Outbox;
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
        var options = CreateRabbitMqOptions();
        var connectionFactory = CreateConnectionFactory(options);

        using var connection = connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();
        var queue = DeclareTestQueue(channel, options);
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
                    Assert.True(Guid.TryParse(message.BasicProperties.MessageId, out var messageId));
                    Assert.Equal(messageId, registeredEvent.EventId);
                    return;
                }
            }

            await Task.Delay(100);
        }

        Assert.Fail($"UserRegisteredEvent for user {userId} was not found in queue {queue}.");
    }

    [RabbitMqStressFact]
    public async Task CreateUser_500ConcurrentRegistrations_ShouldPublish500EventsToRabbitMq()
    {
        const int registrationCount = 500;
        const int maxConcurrentRegistrations = 32;
        var options = CreateRabbitMqOptions();
        var connectionFactory = CreateConnectionFactory(options);

        using var connection = connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();
        var queue = DeclareTestQueue(channel, options);

        using var concurrencyLimiter = new SemaphoreSlim(maxConcurrentRegistrations);
        var responses = await Task.WhenAll(
            Enumerable.Range(1, registrationCount).Select(async index =>
            {
                await concurrencyLimiter.WaitAsync();
                try
                {
                    return await client.PostAsJsonAsync(
                        "/api/v1/users",
                        new CreateUserRequest(
                            $"STRESS-{Guid.NewGuid():N}",
                            $"stress-{index}-{Guid.NewGuid():N}",
                            "Password123!",
                            EntityStatus.Active));
                }
                finally
                {
                    concurrencyLimiter.Release();
                }
            }));
        var userIds = new HashSet<Guid>();

        foreach (var response in responses)
        {
            using (response)
            {
                Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                userIds.Add(await response.Content.ReadFromJsonAsync<Guid>());
            }
        }

        Assert.Equal(registrationCount, userIds.Count);

        var receivedUserIds = new Dictionary<Guid, int>();
        var receivedEventIds = new HashSet<Guid>();
        var receivedMessageCount = 0;
        DateTime? quietPeriodEndsAt = null;
        var deadline = DateTime.UtcNow.AddMinutes(2);
        while (DateTime.UtcNow < deadline)
        {
            var message = channel.BasicGet(queue, autoAck: true);
            if (message is not null)
            {
                receivedMessageCount++;
                var payload = Encoding.UTF8.GetString(message.Body.ToArray());
                var registeredEvent = JsonSerializer.Deserialize<UserRegisteredEventPayload>(payload);
                Assert.NotNull(registeredEvent);
                Assert.Equal("UserRegisteredEvent", message.RoutingKey);
                Assert.True(Guid.TryParse(message.BasicProperties.MessageId, out var messageId));
                Assert.Equal(messageId, registeredEvent.EventId);
                Assert.True(userIds.Contains(registeredEvent.UserId), $"Unexpected user {registeredEvent.UserId} was published.");
                Assert.True(receivedEventIds.Add(registeredEvent.EventId), $"Event {registeredEvent.EventId} was published more than once.");
                receivedUserIds.TryGetValue(registeredEvent.UserId, out var occurrenceCount);
                receivedUserIds[registeredEvent.UserId] = occurrenceCount + 1;

                if (receivedUserIds.Count == userIds.Count && quietPeriodEndsAt is null)
                {
                    quietPeriodEndsAt = DateTime.UtcNow.AddSeconds(2);
                }
            }
            else
            {
                if (receivedUserIds.Count == userIds.Count && quietPeriodEndsAt is null)
                {
                    quietPeriodEndsAt = DateTime.UtcNow.AddSeconds(2);
                }

                if (quietPeriodEndsAt is not null && DateTime.UtcNow >= quietPeriodEndsAt)
                {
                    break;
                }

                await Task.Delay(100);
            }
        }

        Assert.Equal(registrationCount, receivedMessageCount);
        Assert.Equal(registrationCount, receivedUserIds.Count);
        Assert.Equal(registrationCount, receivedEventIds.Count);
        Assert.All(receivedUserIds.Values, occurrenceCount => Assert.Equal(1, occurrenceCount));
        Assert.True(userIds.SetEquals(receivedUserIds.Keys));
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

    private static RabbitMqOptions CreateRabbitMqOptions()
    {
        var defaults = new RabbitMqOptions();
        return new RabbitMqOptions
        {
            Host = ReadEnvironment("RabbitMq__Host", defaults.Host),
            Port = ReadPort(defaults.Port),
            UserName = ReadEnvironment("RabbitMq__UserName", defaults.UserName),
            Password = ReadEnvironment("RabbitMq__Password", defaults.Password),
            Exchange = ReadEnvironment("RabbitMq__Exchange", defaults.Exchange),
            Queue = ReadEnvironment("RabbitMq__Queue", defaults.Queue),
            UnroutedExchange = ReadEnvironment("RabbitMq__UnroutedExchange", defaults.UnroutedExchange),
            UnroutedQueue = ReadEnvironment("RabbitMq__UnroutedQueue", defaults.UnroutedQueue)
        };
    }

    private static ConnectionFactory CreateConnectionFactory(RabbitMqOptions options) => new()
    {
        HostName = options.Host,
        Port = options.Port,
        UserName = options.UserName,
        Password = options.Password
    };

    private static string DeclareTestQueue(IModel channel, RabbitMqOptions options)
    {
        channel.ExchangeDeclare(options.UnroutedExchange, ExchangeType.Fanout, durable: true, autoDelete: false);
        channel.ExchangeDeclare(
            options.Exchange,
            ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: new Dictionary<string, object> { ["alternate-exchange"] = options.UnroutedExchange });
        var queue = channel.QueueDeclare().QueueName;
        channel.QueueBind(queue, options.Exchange, nameof(UserRegisteredEvent));
        return queue;
    }

    private static string ReadEnvironment(string name, string fallback) =>
        Environment.GetEnvironmentVariable(name) is { Length: > 0 } value ? value : fallback;

    private static int ReadPort(int fallback) =>
        int.TryParse(Environment.GetEnvironmentVariable("RabbitMq__Port"), out var port) ? port : fallback;

    private sealed record UserRegisteredEventPayload(Guid EventId, Guid UserId, string Code, string Login, DateTimeOffset OccurredAt);
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

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
internal sealed class RabbitMqStressFactAttribute : FactAttribute
{
    public RabbitMqStressFactAttribute()
    {
        var rabbitEnabled = bool.TryParse(Environment.GetEnvironmentVariable("RabbitMq__Enabled"), out var enabled) && enabled;
        var stressEnabled = bool.TryParse(Environment.GetEnvironmentVariable("RabbitMq__RunStressTests"), out var runStress) && runStress;
        if (!rabbitEnabled || !stressEnabled)
        {
            Skip = "RabbitMQ stress tests require RabbitMq__Enabled=true and RabbitMq__RunStressTests=true.";
        }
    }
}
