using System.Net;
using System.Net.Http.Json;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystem.Tests.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class UnitsIntegrationTests(EmployeeManagementApiFactory factory) : IAsyncLifetime
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
    public async Task UnitsEndpoints_ShouldCreateListGetUpdateAndIncludeEmployees()
    {
        var unitId = await api.CreateUnitAsync("UNIT-001", "Headquarters");
        var userId = await api.CreateUserAsync("USR-001", "employee.user");
        var employeeId = await api.CreateEmployeeAsync("EMP-001", "Employee One", userId, unitId);

        var unit = await client.GetFromJsonAsync<UnitDto>($"/api/v1/units/{unitId}");
        var updateResponse = await client.PatchAsJsonAsync(
            $"/api/v1/units/{unitId}",
            new UpdateUnitRequest("Updated Headquarters", EntityStatus.Inactive));
        var units = await client.GetFromJsonAsync<IReadOnlyCollection<UnitDto>>("/api/v1/units");

        Assert.NotNull(unit);
        Assert.Equal("Headquarters", unit.Name);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        Assert.NotNull(units);
        var updated = Assert.Single(units, candidate => candidate.Id == unitId);
        Assert.Equal("Updated Headquarters", updated.Name);
        Assert.Equal(EntityStatus.Inactive, updated.Status);
        var employee = Assert.Single(updated.Employees);
        Assert.Equal(employeeId, employee.Id);
    }

    [Fact]
    public async Task CreateUnit_WithDuplicateCode_ShouldReturnConflict()
    {
        await api.CreateUnitAsync("UNIT-001", "Headquarters");

        var response = await client.PostAsJsonAsync("/api/v1/units", new CreateUnitRequest("UNIT-001", "Branch"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task MissingOrInvalidUnitRequests_ShouldReturnExpectedProblemDetails()
    {
        var missingId = Guid.NewGuid();

        var notFound = await client.GetAsync($"/api/v1/units/{missingId}");
        var invalid = await client.PostAsJsonAsync("/api/v1/units", new CreateUnitRequest(string.Empty, string.Empty));

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
