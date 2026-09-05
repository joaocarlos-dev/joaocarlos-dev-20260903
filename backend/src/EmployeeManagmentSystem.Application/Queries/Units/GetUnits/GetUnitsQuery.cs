using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.DTOs;
using MediatR;

namespace EmployeeManagmentSystem.Application.Queries.Units.GetUnits;

public sealed record GetUnitsQuery : IQuery<IReadOnlyCollection<UnitDto>>;

internal sealed class GetUnitsQueryHandler(
    IUnitRepository unitRepository,
    IEmployeeRepository employeeRepository) : IRequestHandler<GetUnitsQuery, IReadOnlyCollection<UnitDto>>
{
    public async Task<IReadOnlyCollection<UnitDto>> Handle(
        GetUnitsQuery request,
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
