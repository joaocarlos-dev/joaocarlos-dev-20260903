using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Tests.IntegrationTests;

internal sealed class ApiTestClient(HttpClient client, EmployeeManagementApiFactory factory)
{
    public async Task AuthenticateAsAdminAsync()
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest(factory.AdminLogin, factory.AdminPassword));
        response.EnsureSuccessStatusCode();

        var authentication = await response.Content.ReadFromJsonAsync<AuthenticationDto>();
        Assert.NotNull(authentication);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authentication.AccessToken);
    }

    public async Task<Guid> CreateUserAsync(
        string code,
        string login,
        string password = "Password123!",
        EntityStatus status = EntityStatus.Active)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/users",
            new CreateUserRequest(code, login, password, status));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task<Guid> CreateUnitAsync(string code, string name)
    {
        var response = await client.PostAsJsonAsync("/api/v1/units", new CreateUnitRequest(code, name));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task<Guid> CreateEmployeeAsync(string code, string name, Guid userId, Guid unitId)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new CreateEmployeeRequest(code, name, userId, unitId));
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Guid>();
    }
}

internal sealed record LoginRequest(string Login, string Password);

internal sealed record CreateUserRequest(string Code, string Login, string Password, EntityStatus Status);

internal sealed record UpdateUserRequest(string? Password, EntityStatus? Status);

internal sealed record CreateUnitRequest(string Code, string Name);

internal sealed record UpdateUnitRequest(string? Name, EntityStatus? Status);

internal sealed record CreateEmployeeRequest(string Code, string Name, Guid UserId, Guid UnitId);

internal sealed record UpdateEmployeeRequest(string? Name, Guid? UnitId);
