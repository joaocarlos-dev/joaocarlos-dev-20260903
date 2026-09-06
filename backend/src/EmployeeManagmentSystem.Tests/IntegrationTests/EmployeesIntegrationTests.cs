using System.Net;
using System.Net.Http.Json;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystem.Tests.IntegrationTests;

[Collection(IntegrationTestCollection.Name)]
public sealed class EmployeesIntegrationTests(EmployeeManagementApiFactory factory) : IAsyncLifetime
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
    public async Task EmployeesEndpoints_ShouldCreateListGetUpdateTransferAndSoftDeleteEmployees()
    {
        var firstUnitId = await api.CreateUnitAsync("UNIT-001", "Headquarters");
        var secondUnitId = await api.CreateUnitAsync("UNIT-002", "Branch");
        var userId = await api.CreateUserAsync("USR-001", "employee.user");
        var employeeId = await api.CreateEmployeeAsync("EMP-001", "Employee One", userId, firstUnitId);

        var employee = await client.GetFromJsonAsync<EmployeeDto>($"/api/v1/employees/{employeeId}");
        var updateResponse = await client.PatchAsJsonAsync(
            $"/api/v1/employees/{employeeId}",
            new UpdateEmployeeRequest("Employee Updated", secondUnitId));
        var updatedEmployee = await client.GetFromJsonAsync<EmployeeDto>($"/api/v1/employees/{employeeId}");
        var deleteResponse = await client.DeleteAsync($"/api/v1/employees/{employeeId}");
        var employeesAfterDelete = await client.GetFromJsonAsync<IReadOnlyCollection<EmployeeDto>>("/api/v1/employees");
        var getDeleted = await client.GetAsync($"/api/v1/employees/{employeeId}");

        Assert.NotNull(employee);
        Assert.Equal(firstUnitId, employee.UnitId);
        Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);
        Assert.NotNull(updatedEmployee);
        Assert.Equal("Employee Updated", updatedEmployee.Name);
        Assert.Equal(secondUnitId, updatedEmployee.UnitId);
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.NotNull(employeesAfterDelete);
        Assert.DoesNotContain(employeesAfterDelete, candidate => candidate.Id == employeeId);
        Assert.Equal(HttpStatusCode.NotFound, getDeleted.StatusCode);
    }

    [Fact]
    public async Task CreateEmployee_WithDuplicateCodeLinkedUserOrMissingResources_ShouldReturnExpectedStatuses()
    {
        var unitId = await api.CreateUnitAsync("UNIT-001", "Headquarters");
        var userId = await api.CreateUserAsync("USR-001", "employee.user");
        await api.CreateEmployeeAsync("EMP-001", "Employee One", userId, unitId);

        var duplicateCode = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new CreateEmployeeRequest("EMP-001", "Employee Two", Guid.NewGuid(), unitId));
        var linkedUser = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new CreateEmployeeRequest("EMP-002", "Employee Two", userId, unitId));
        var missingUser = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new CreateEmployeeRequest("EMP-003", "Employee Three", Guid.NewGuid(), unitId));
        var missingUnit = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new CreateEmployeeRequest("EMP-004", "Employee Four", Guid.NewGuid(), Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Conflict, duplicateCode.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, linkedUser.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingUser.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingUnit.StatusCode);
    }

    [Fact]
    public async Task CreateOrTransferEmployee_ToInactiveUnit_ShouldReturnConflict()
    {
        var activeUnitId = await api.CreateUnitAsync("UNIT-001", "Headquarters");
        var inactiveUnitId = await api.CreateUnitAsync("UNIT-002", "Branch");
        await client.PatchAsJsonAsync(
            $"/api/v1/units/{inactiveUnitId}",
            new UpdateUnitRequest(null, EntityStatus.Inactive));
        var firstUserId = await api.CreateUserAsync("USR-001", "first.employee");
        var secondUserId = await api.CreateUserAsync("USR-002", "second.employee");
        var employeeId = await api.CreateEmployeeAsync("EMP-001", "Employee One", firstUserId, activeUnitId);

        var createInInactiveUnit = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new CreateEmployeeRequest("EMP-002", "Employee Two", secondUserId, inactiveUnitId));
        var transferToInactiveUnit = await client.PatchAsJsonAsync(
            $"/api/v1/employees/{employeeId}",
            new UpdateEmployeeRequest(null, inactiveUnitId));

        Assert.Equal(HttpStatusCode.Conflict, createInInactiveUnit.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, transferToInactiveUnit.StatusCode);
    }

    [Fact]
    public async Task MissingOrInvalidEmployeeRequests_ShouldReturnExpectedProblemDetails()
    {
        var missingId = Guid.NewGuid();

        var notFound = await client.GetAsync($"/api/v1/employees/{missingId}");
        var invalid = await client.PostAsJsonAsync(
            "/api/v1/employees",
            new CreateEmployeeRequest(string.Empty, string.Empty, Guid.Empty, Guid.Empty));

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
