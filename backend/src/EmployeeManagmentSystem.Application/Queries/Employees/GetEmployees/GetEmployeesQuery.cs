using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.DTOs;
using MediatR;

namespace EmployeeManagmentSystem.Application.Queries.Employees.GetEmployees;

public sealed record GetEmployeesQuery : IQuery<IReadOnlyCollection<EmployeeDto>>;

internal sealed class GetEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<GetEmployeesQuery, IReadOnlyCollection<EmployeeDto>>
{
    public async Task<IReadOnlyCollection<EmployeeDto>> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.ListAsync(cancellationToken);
        return employees.Select(EmployeeDto.FromEntity).ToArray();
    }
}
