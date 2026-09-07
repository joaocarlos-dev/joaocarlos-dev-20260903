using EmployeeManagmentSystem.Application;
using EmployeeManagmentSystem.Application.Commands.Employees.CreateEmployee;
using EmployeeManagmentSystem.Application.Commands.Employees.DeleteEmployee;
using EmployeeManagmentSystem.Application.Commands.Employees.UpdateEmployee;
using EmployeeManagmentSystem.Application.Commands.Units.CreateUnit;
using EmployeeManagmentSystem.Application.Commands.Units.UpdateUnit;
using EmployeeManagmentSystem.Application.Commands.Users.CreateUser;
using EmployeeManagmentSystem.Application.Commands.Users.UpdateUser;
using EmployeeManagmentSystem.Application.Queries.Authentication.Authenticate;
using EmployeeManagmentSystem.Application.Queries.Employees.GetEmployee;
using EmployeeManagmentSystem.Application.Queries.Employees.GetEmployees;
using EmployeeManagmentSystem.Application.Queries.Units.GetUnit;
using EmployeeManagmentSystem.Application.Queries.Units.GetUnits;
using EmployeeManagmentSystem.Application.Queries.Users.GetUser;
using EmployeeManagmentSystem.Application.Queries.Users.GetUsers;
using EmployeeManagmentSystem.Domain.Enums;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application.Validators;

public sealed class ApplicationValidatorTests
{
    [Fact]
    public async Task AuthenticateQueryValidator_ShouldAcceptValidRequestAndRejectInvalidRequest()
    {
        await AssertValidAsync(new AuthenticateQuery("admin", "password123"));
        await AssertInvalidAsync(new AuthenticateQuery(string.Empty, string.Empty));
    }

    [Fact]
    public async Task UserValidators_ShouldAcceptValidRequestsAndRejectInvalidRequests()
    {
        await AssertValidAsync(new CreateUserCommand("USR-001", "admin", "password123", EntityStatus.Active));
        await AssertValidAsync(new CreateUserCommand("USR-002", "admin2", "password123", EntityStatus.Active, UserRole.Administrator));
        await AssertValidAsync(new UpdateUserCommand(Guid.NewGuid(), "password123", EntityStatus.Inactive));
        await AssertValidAsync(new GetUserQuery(Guid.NewGuid()));
        await AssertValidAsync(new GetUsersQuery(EntityStatus.Active));

        await AssertInvalidAsync(new CreateUserCommand(string.Empty, string.Empty, string.Empty, (EntityStatus)999));
        await AssertInvalidAsync(new CreateUserCommand("   ", "valid-login", "password123", EntityStatus.Active));
        await AssertInvalidAsync(new CreateUserCommand("USR-003", "   ", "password123", EntityStatus.Active));
        await AssertInvalidAsync(new CreateUserCommand("USR-003", "user3", "        ", EntityStatus.Active));
        await AssertInvalidAsync(new CreateUserCommand("USR-002", "admin2", "password123", EntityStatus.Active, (UserRole)999));
        await AssertInvalidAsync(new UpdateUserCommand(Guid.NewGuid(), "        ", null));
        await AssertInvalidAsync(new UpdateUserCommand(Guid.Empty, null, null));
        await AssertInvalidAsync(new UpdateUserCommand(Guid.NewGuid(), null, (EntityStatus)999));
        await AssertInvalidAsync(new GetUserQuery(Guid.Empty));
        await AssertInvalidAsync(new GetUsersQuery((EntityStatus)999));
        await AssertInvalidAsync(new AuthenticateQuery("        ", "        "));
    }

    [Fact]
    public async Task EmployeeValidators_ShouldAcceptValidRequestsAndRejectInvalidRequests()
    {
        await AssertValidAsync(new CreateEmployeeCommand("EMP-001", "Employee", Guid.NewGuid(), Guid.NewGuid()));
        await AssertValidAsync(new UpdateEmployeeCommand(Guid.NewGuid(), "Employee Updated", null));
        await AssertValidAsync(new UpdateEmployeeCommand(Guid.NewGuid(), null, Guid.NewGuid()));
        await AssertValidAsync(new DeleteEmployeeCommand(Guid.NewGuid()));
        await AssertValidAsync(new GetEmployeeQuery(Guid.NewGuid()));
        await AssertValidAsync(new GetEmployeesQuery());

        await AssertInvalidAsync(new CreateEmployeeCommand(string.Empty, string.Empty, Guid.Empty, Guid.Empty));
        await AssertInvalidAsync(new UpdateEmployeeCommand(Guid.Empty, null, null));
        await AssertInvalidAsync(new UpdateEmployeeCommand(Guid.NewGuid(), string.Empty, null));
        await AssertInvalidAsync(new UpdateEmployeeCommand(Guid.NewGuid(), null, Guid.Empty));
        await AssertInvalidAsync(new DeleteEmployeeCommand(Guid.Empty));
        await AssertInvalidAsync(new GetEmployeeQuery(Guid.Empty));
    }

    [Fact]
    public async Task UnitValidators_ShouldAcceptValidRequestsAndRejectInvalidRequests()
    {
        await AssertValidAsync(new CreateUnitCommand("UNIT-001", "Headquarters"));
        await AssertValidAsync(new UpdateUnitCommand(Guid.NewGuid(), "Branch", null));
        await AssertValidAsync(new UpdateUnitCommand(Guid.NewGuid(), null, EntityStatus.Inactive));
        await AssertValidAsync(new GetUnitQuery(Guid.NewGuid()));
        await AssertValidAsync(new GetUnitsQuery());

        await AssertInvalidAsync(new CreateUnitCommand(string.Empty, string.Empty));
        await AssertInvalidAsync(new UpdateUnitCommand(Guid.Empty, null, null));
        await AssertInvalidAsync(new UpdateUnitCommand(Guid.NewGuid(), string.Empty, null));
        await AssertInvalidAsync(new UpdateUnitCommand(Guid.NewGuid(), null, (EntityStatus)999));
        await AssertInvalidAsync(new GetUnitQuery(Guid.Empty));
    }

    private static async Task AssertValidAsync<T>(T request)
    {
        await using var provider = CreateProvider();
        var validator = provider.GetRequiredService<IValidator<T>>();

        var result = await validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    private static async Task AssertInvalidAsync<T>(T request)
    {
        await using var provider = CreateProvider();
        var validator = provider.GetRequiredService<IValidator<T>>();

        var result = await validator.ValidateAsync(request);

        Assert.False(result.IsValid);
    }

    private static ServiceProvider CreateProvider()
    {
        var services = new ServiceCollection();
        services.AddApplicationDependencies();
        return services.BuildServiceProvider();
    }
}
