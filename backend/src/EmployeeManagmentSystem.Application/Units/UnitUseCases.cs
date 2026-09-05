using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;
using FluentValidation;
using MediatR;
using DomainUnit = EmployeeManagmentSystem.Domain.Entities.Unit;

namespace EmployeeManagmentSystem.Application.Units;

public sealed record CreateUnitCommand(string Code, string Name) : ICommand<Guid>;

public sealed record UpdateUnitCommand(Guid Id, string? Name, EntityStatus? Status) : ICommand;

public sealed record GetUnitQuery(Guid Id) : IQuery<UnitDto>;

public sealed record ListUnitsQuery : IQuery<IReadOnlyCollection<UnitDto>>;

internal sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty();
        RuleFor(command => command.Name).NotEmpty();
    }
}

internal sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().When(command => command.Name is not null);
        RuleFor(command => command.Status).IsInEnum().When(command => command.Status.HasValue);
        RuleFor(command => command).Must(command => command.Name is not null || command.Status.HasValue)
            .WithMessage("At least one unit field must be provided for update.");
    }
}

internal sealed class GetUnitQueryValidator : AbstractValidator<GetUnitQuery>
{
    public GetUnitQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}

internal sealed class CreateUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUnitCommand, Guid>
{
    public async Task<Guid> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();

        if (await unitRepository.CodeExistsAsync(code, cancellationToken))
        {
            throw new ApplicationConflictException("The unit code is already in use.");
        }

        var unit = new DomainUnit(code, request.Name);
        await unitRepository.AddAsync(unit, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return unit.Id;
    }
}

internal sealed class UpdateUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUnitCommand>
{
    public async Task Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await unitRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DomainUnit), request.Id);

        if (request.Name is not null)
        {
            unit.UpdateName(request.Name);
        }

        if (request.Status.HasValue)
        {
            unit.ChangeStatus(request.Status.Value);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class GetUnitQueryHandler(
    IUnitRepository unitRepository,
    IEmployeeRepository employeeRepository) : IRequestHandler<GetUnitQuery, UnitDto>
{
    public async Task<UnitDto> Handle(GetUnitQuery request, CancellationToken cancellationToken)
    {
        var unit = await unitRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DomainUnit), request.Id);
        var employees = await employeeRepository.ListByUnitIdsAsync([unit.Id], cancellationToken);

        return UnitDto.FromEntity(unit, employees);
    }
}

internal sealed class ListUnitsQueryHandler(
    IUnitRepository unitRepository,
    IEmployeeRepository employeeRepository) : IRequestHandler<ListUnitsQuery, IReadOnlyCollection<UnitDto>>
{
    public async Task<IReadOnlyCollection<UnitDto>> Handle(
        ListUnitsQuery request,
        CancellationToken cancellationToken)
    {
        var units = await unitRepository.ListAsync(cancellationToken);
        var unitIds = units.Select(unit => unit.Id).ToArray();
        var employees = await employeeRepository.ListByUnitIdsAsync(unitIds, cancellationToken);
        var employeesByUnit = employees.ToLookup(employee => employee.UnitId);
        var results = new List<UnitDto>(units.Count);

        foreach (var unit in units)
        {
            results.Add(UnitDto.FromEntity(unit, employeesByUnit[unit.Id].ToArray()));
        }

        return results;
    }
}
