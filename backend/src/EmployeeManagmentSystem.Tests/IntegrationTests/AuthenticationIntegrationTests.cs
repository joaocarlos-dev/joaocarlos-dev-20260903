using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Tests.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthenticationIntegrationTests(EmployeeManagementApiFactory factory) : IAsyncLifetime
{
    public Task InitializeAsync() => factory.ResetDatabaseAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task HealthCheck_WithoutToken_ShouldReturnOk()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnUsableToken()
    {
        using var client = factory.CreateClient();
        var api = new ApiTestClient(client, factory);

        await api.AuthenticateAsAdminAsync();
        var response = await client.GetAsync("/api/v1/users");
        var users = await response.Content.ReadFromJsonAsync<IReadOnlyCollection<UserDto>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(users!, user => user.Login == factory.AdminLogin && user.Role == UserRole.Administrator);

        var secondAdminId = await api.CreateUserAsync(
            "ADM-002",
            "second.admin",
            role: UserRole.Administrator);
        var secondAdmin = await client.GetFromJsonAsync<UserDto>($"/api/v1/users/{secondAdminId}");

        Assert.NotNull(secondAdmin);
        Assert.Equal(UserRole.Administrator, secondAdmin.Role);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorizedProblemDetails()
    {
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(factory.AdminLogin, "wrong-password"));

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.NotNull(problem);
        Assert.Equal("Authentication failed", problem.Title);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldReturnUnauthorized()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<EmployeeManagementDbContext>();
        var user = await context.Users.SingleAsync(user => user.Login == factory.AdminLogin);
        user.ChangeStatus(EntityStatus.Inactive);
        await context.SaveChangesAsync();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(factory.AdminLogin, factory.AdminPassword));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedRoute_WithoutToken_ShouldReturnUnauthorized()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ProtectedRoute_WithInvalidToken_ShouldReturnUnauthorized()
    {
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");

        var response = await client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConventionalUser_ShouldReceiveForbiddenOnAdministrativeMutations()
    {
        using var adminClient = factory.CreateClient();
        var adminApi = new ApiTestClient(adminClient, factory);
        await adminApi.AuthenticateAsAdminAsync();
        await adminApi.CreateUserAsync("USR-001", "conventional.user");

        using var conventionalClient = factory.CreateClient();
        var conventionalApi = new ApiTestClient(conventionalClient, factory);
        await conventionalApi.AuthenticateAsync("conventional.user", "Password123!");

        var updateResponse = await conventionalClient.PatchAsJsonAsync(
            "/api/v1/users/00000000-0000-0000-0000-000000000001",
            new UpdateUserRequest("NewPassword123!", null));
        var createResponse = await conventionalClient.PostAsJsonAsync(
            "/api/v1/units",
            new CreateUnitRequest("UNIT-001", "Headquarters"));

        Assert.Equal(HttpStatusCode.Forbidden, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, createResponse.StatusCode);
    }

    [Fact]
    public async Task PreviouslyIssuedToken_AfterUserDeactivation_ShouldReturnUnauthorized()
    {
        using var adminClient = factory.CreateClient();
        var adminApi = new ApiTestClient(adminClient, factory);
        await adminApi.AuthenticateAsAdminAsync();
        var userId = await adminApi.CreateUserAsync("USR-001", "session.user");

        using var userClient = factory.CreateClient();
        var userApi = new ApiTestClient(userClient, factory);
        await userApi.AuthenticateAsync("session.user", "Password123!");

        var beforeDeactivation = await userClient.GetAsync("/api/v1/users");
        var updateResponse = await adminClient.PatchAsJsonAsync(
            $"/api/v1/users/{userId}",
            new UpdateUserRequest(null, EntityStatus.Inactive));
        var afterDeactivation = await userClient.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.OK, beforeDeactivation.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, afterDeactivation.StatusCode);
    }
}
