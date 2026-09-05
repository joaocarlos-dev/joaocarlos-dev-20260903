using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Application.DTOs;

public sealed record UnitDto(
    Guid Id,
    string Code,
    string Name,
    EntityStatus Status,
    IReadOnlyCollection<EmployeeDto> Employees,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static UnitDto FromEntity(Unit unit, IReadOnlyCollection<Employee> employees) =>
        new(
            unit.Id,
            unit.Code,
            unit.Name,
            unit.Status,
            employees.Select(EmployeeDto.FromEntity).ToArray(),
            unit.CreatedAt,
            unit.UpdatedAt);
}
