using EmployeeManagmentSystem.Application;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Commands.Units.CreateUnit;
using EmployeeManagmentSystem.Application.Commands.Units.UpdateUnit;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.Queries.Units.GetUnit;
using EmployeeManagmentSystem.Application.Queries.Units.GetUnits;
using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using DomainUnit = EmployeeManagmentSystem.Domain.Entities.Unit;

namespace EmployeeManagmentSystem.Tests.UnitTests.Application.Units;

public sealed class UnitOperationsTests
{
    [Fact]
    public async Task CreateUnit_WithUniqueData_ShouldPersistUnit()
    {
        var unitRepository = new FakeUnitRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUnitCommand("UNIT-001", "Headquarters");

        var id = await mediator.Send(command);

        var unit = Assert.Single(unitRepository.Units);
        Assert.Equal(id, unit.Id);
        Assert.Equal("UNIT-001", unit.Code);
        Assert.Equal("Headquarters", unit.Name);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUnit_WithCodeSurroundedBySpaces_ShouldDetectDuplicate()
    {
        var unitRepository = new FakeUnitRepository();
        unitRepository.Units.Add(new DomainUnit("UNIT-001", "Headquarters"));
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUnitCommand(" UNIT-001 ", "Branch");

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ApplicationConflictException>(action);
        Assert.Single(unitRepository.Units);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task UpdateUnit_WithStatus_ShouldPersistInactiveStatus()
    {
        var unitRepository = new FakeUnitRepository();
        var unit = new DomainUnit("UNIT-001", "Headquarters");
        unitRepository.Units.Add(unit);
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new UpdateUnitCommand(unit.Id, null, EntityStatus.Inactive);

        await mediator.Send(command);

        Assert.Equal(EntityStatus.Inactive, unit.Status);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUnit_WithNameExceedingMaximumLength_ShouldFailValidationAndNotPersist()
    {
        var unitRepository = new FakeUnitRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new CreateUnitCommand("UNIT-001", new string('a', EntityFieldLengths.Name + 1));

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<ValidationException>(action);
        Assert.Empty(unitRepository.Units);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task CreateUnit_WithTrimmedNameAtMaximumLength_ShouldPersistNormalizedName()
    {
        var unitRepository = new FakeUnitRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var name = new string('a', EntityFieldLengths.Name);
        var command = new CreateUnitCommand("UNIT-001", $" {name} ");

        await mediator.Send(command);

        var unit = Assert.Single(unitRepository.Units);
        Assert.Equal(name, unit.Name);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task UpdateUnit_WithTrimmedNameAtMaximumLength_ShouldPersistNormalizedName()
    {
        var unitRepository = new FakeUnitRepository();
        var unit = new DomainUnit("UNIT-001", "Headquarters");
        unitRepository.Units.Add(unit);
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var name = new string('a', EntityFieldLengths.Name);
        var command = new UpdateUnitCommand(unit.Id, $" {name} ", null);

        await mediator.Send(command);

        Assert.Equal(name, unit.Name);
        Assert.Equal(1, unitOfWork.SaveCalls);
    }

    [Fact]
    public async Task ListUnits_WithEmployees_ShouldLoadEmployeesInSingleBatch()
    {
        var unitRepository = new FakeUnitRepository();
        var firstUnit = new DomainUnit("UNIT-001", "Headquarters");
        var secondUnit = new DomainUnit("UNIT-002", "Branch");
        unitRepository.Units.AddRange([firstUnit, secondUnit]);
        var employeeRepository = new FakeEmployeeRepository();
        employeeRepository.Employees.Add(new Employee("EMP-001", "First", Guid.NewGuid(), firstUnit));
        employeeRepository.Employees.Add(new Employee("EMP-002", "Second", Guid.NewGuid(), secondUnit));
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new GetUnitsQuery();

        var result = await mediator.Send(query);

        Assert.Equal(2, result.Count);
        Assert.All(result, unit => Assert.Single(unit.Employees));
        Assert.Equal(1, employeeRepository.ListByUnitIdsCalls);
    }

    [Fact]
    public async Task GetUnit_WithExistingUnit_ShouldReturnUnit()
    {
        var unitRepository = new FakeUnitRepository();
        var existingUnit = new DomainUnit("UNIT-001", "Headquarters");
        unitRepository.Units.Add(existingUnit);
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new GetUnitQuery(existingUnit.Id);

        var result = await mediator.Send(query);

        Assert.Equal(existingUnit.Id, result.Id);
        Assert.Equal(existingUnit.Name, result.Name);
    }

    [Fact]
    public async Task GetUnit_WithMissingUnit_ShouldThrowNotFound()
    {
        var unitRepository = new FakeUnitRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var query = new GetUnitQuery(Guid.NewGuid());

        var action = () => mediator.Send(query);

        await Assert.ThrowsAsync<NotFoundException>(action);
    }

    [Fact]
    public async Task UpdateUnit_WithMissingUnit_ShouldThrowNotFoundAndNotPersist()
    {
        var unitRepository = new FakeUnitRepository();
        var employeeRepository = new FakeEmployeeRepository();
        var unitOfWork = new FakeUnitOfWork();
        await using var provider = CreateProvider(unitRepository, employeeRepository, unitOfWork);
        var mediator = provider.GetRequiredService<IMediator>();
        var command = new UpdateUnitCommand(Guid.NewGuid(), "Updated", null);

        var action = () => mediator.Send(command);

        await Assert.ThrowsAsync<NotFoundException>(action);
        Assert.Equal(0, unitOfWork.SaveCalls);
    }

    private static ServiceProvider CreateProvider(
        FakeUnitRepository unitRepository,
        FakeEmployeeRepository employeeRepository,
        FakeUnitOfWork unitOfWork)
    {
        var services = new ServiceCollection();
        services.AddApplicationDependencies();
        services.AddSingleton<IUnitRepository>(unitRepository);
        services.AddSingleton<IEmployeeRepository>(employeeRepository);
        services.AddSingleton<IUnitOfWork>(unitOfWork);

        return services.BuildServiceProvider();
    }
}
