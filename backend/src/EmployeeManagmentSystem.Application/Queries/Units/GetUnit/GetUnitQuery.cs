using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.DTOs;
using MediatR;
using DomainUnit = EmployeeManagmentSystem.Domain.Entities.Unit;

namespace EmployeeManagmentSystem.Application.Queries.Units.GetUnit;

public sealed record GetUnitQuery(Guid Id) : IQuery<UnitDto>;

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
