using EmployeeManagmentSystem.Domain.Entities;

namespace EmployeeManagmentSystem.Application.DTOs;

public sealed record EmployeeDto(
    Guid Id,
    string Code,
    string Name,
    Guid UserId,
    Guid UnitId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static EmployeeDto FromEntity(Employee employee) =>
        new(
            employee.Id,
            employee.Code,
            employee.Name,
            employee.UserId,
            employee.UnitId,
            employee.CreatedAt,
            employee.UpdatedAt);
}
