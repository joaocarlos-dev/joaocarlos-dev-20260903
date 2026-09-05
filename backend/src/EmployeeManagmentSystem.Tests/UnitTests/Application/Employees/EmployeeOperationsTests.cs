using EmployeeManagmentSystem.Application;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Commands.Employees.CreateEmployee;
using EmployeeManagmentSystem.Application.Commands.Employees.DeleteEmployee;
using EmployeeManagmentSystem.Application.Commands.Employees.UpdateEmployee;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DomainUnit = EmployeeManagmentSystem.Domain.Entities.Unit;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application.Employees;

public sealed class EmployeeOperationsTests
{
    [Fact]
    public async Task CreateEmployee_WithAvailableUserAndActiveUnit_ShouldPersistEmployee()
    {
        var userRepository = new FakeUserRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitRepository = new FakeUnitRepository();
        var unitOfWork = new FakeUnitOfWork();
        var user = new User("USR-001", "admin", "hashed:password123");
        var unit = new DomainUnit("UNIT-001", "Headquarters");
        userRepository.Users.Add(user);
        unitRepository.Units.Add(unit);
        await using var provider = CreateProvider(userRepository, employeeRepository, unitRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateEmployeeCommand("EMP-001", "Employee", user.Id, unit.Id);

        var id = await mediator.Send(command);

        var employee = Assert.Single(employeeRepository.Employees);
        Assert.Equal(id, employee.Id);
        Assert.Equal(user.Id, employee.UserId);
        Assert.Equal(unit.Id, employee.UnitId);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateEmployee_WithLinkedUser_ShouldThrowConflictAndNotPersist()
    {
        var userRepository = new FakeUserRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitRepository = new FakeUnitRepository();
        var unitOfWork = new FakeUnitOfWork();
        var user = new User("USR-001", "admin", "hashed:password123");
        var unit = new DomainUnit("UNIT-001", "Headquarters");
        userRepository.Users.Add(user);
        unitRepository.Units.Add(unit);
        employeeRepository.Employees.Add(new Employee("EMP-001", "First", user.Id, unit));
        await using var provider = CreateProvider(userRepository, employeeRepository, unitRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateEmployeeCommand("EMP-002", "Second", user.Id, unit.Id);

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ApplicationConflictException>(action);
        Assert.Single(employeeRepository.Employees);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateEmployee_WithNameExceedingMaximumLength_ShouldFailValidationAndNotPersist()
    {
        var userRepository = new FakeUserRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitRepository = new FakeUnitRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(userRepository, employeeRepository, unitRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateEmployeeCommand(
            "EMP-001",
            new string('a', EntityFieldLengths.Name + 1),
            Guid.NewGuid(),
            Guid.NewGuid());

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Empty(employeeRepository.Employees);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateEmployee_WithTrimmedNameAtMaximumLength_ShouldPersistNormalizedName()
    {
        var userRepository = new FakeUserRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitRepository = new FakeUnitRepository();
        var unitOfWork = new FakeUnitOfWork();
        var user = new User("USR-001", "admin", "hashed:password123");
        var unit = new DomainUnit("UNIT-001", "Headquarters");
        userRepository.Users.Add(user);
        unitRepository.Units.Add(unit);
        await using var provider = CreateProvider(userRepository, employeeRepository, unitRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var name = new string('a', EntityFieldLengths.Name);
        var command = new CreateEmployeeCommand("EMP-001", $" {name} ", user.Id, unit.Id);

        await mediator.Send(command);

        var employee = Assert.Single(employeeRepository.Employees);
        Assert.Equal(name, employee.Name);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task UpdateEmployee_WithTrimmedNameAtMaximumLength_ShouldPersistNormalizedName()
    {
        var userRepository = new FakeUserRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitRepository = new FakeUnitRepository();
        var unitOfWork = new FakeUnitOfWork();
        var unit = new DomainUnit("UNIT-001", "Headquarters");
        var employee = new Employee("EMP-001", "Employee", Guid.NewGuid(), unit);
        employeeRepository.Employees.Add(employee);
        await using var provider = CreateProvider(userRepository, employeeRepository, unitRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var name = new string('a', EntityFieldLengths.Name);
        var command = new UpdateEmployeeCommand(employee.Id, $" {name} ", null);

        await mediator.Send(command);

        Assert.Equal(name, employee.Name);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateEmployee_WithUserLinkedToDeletedEmployee_ShouldKeepUniqueLink()
    {
        var userRepository = new FakeUserRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitRepository = new FakeUnitRepository();
        var unitOfWork = new FakeUnitOfWork();
        var user = new User("USR-001", "admin", "hashed:password123");
        var unit = new DomainUnit("UNIT-001", "Headquarters");
        var deletedEmployee = new Employee("EMP-001", "First", user.Id, unit);
        deletedEmployee.Delete();
        userRepository.Users.Add(user);
        unitRepository.Units.Add(unit);
        employeeRepository.Employees.Add(deletedEmployee);
        await using var provider = CreateProvider(userRepository, employeeRepository, unitRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateEmployeeCommand("EMP-002", "Second", user.Id, unit.Id);

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ApplicationConflictException>(action);
        Assert.Single(employeeRepository.Employees);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task DeleteEmployee_WithExistingEmployee_ShouldApplySoftDelete()
    {
        var userRepository = new FakeUserRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitRepository = new FakeUnitRepository();
        var unitOfWork = new FakeUnitOfWork();
        var unit = new DomainUnit("UNIT-001", "Headquarters");
        var employee = new Employee("EMP-001", "Employee", Guid.NewGuid(), unit);
        employeeRepository.Employees.Add(employee);
        await using var provider = CreateProvider(userRepository, employeeRepository, unitRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new DeleteEmployeeCommand(employee.Id);

        await mediator.Send(command);

        Assert.True(employee.IsDeleted);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    private static ServiceProvider CreateProvider(
        FakeUserRepository userRepository,
        FakeEmployeeRepository employeeRepository,
        FakeUnitRepository unitRepository,
        FakeUnitOfWork unitOfWork)
    {
        var services = new ServiceCollection();
        services.AddApplicationDependencies();
        services.AddSingleton<IUserRepository>(userRepository);
        services.AddSingleton<IEmployeeRepository>(employeeRepository);
        services.AddSingleton<IUnitRepository>(unitRepository);
        services.AddSingleton<IUnitOfWork>(unitOfWork);

        return services.BuildServiceProvider();
    }
}
