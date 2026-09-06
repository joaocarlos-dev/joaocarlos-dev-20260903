using System.Net;
using System.Net.Http.Json;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

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
}
