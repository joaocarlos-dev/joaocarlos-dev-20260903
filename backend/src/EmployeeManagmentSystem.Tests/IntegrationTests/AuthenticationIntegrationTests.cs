using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
    public async Task Login_WithValidCredentials_ShouldReturnUsableToken()
    {
        using var client = factory.CreateClient();
        var api = new ApiTestClient(client, factory);

        await api.AuthenticateAsAdminAsync();
        var response = await client.GetAsync("/api/v1/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
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
}
