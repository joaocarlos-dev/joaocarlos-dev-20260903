using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Entities;
using MediatR;

namespace EmployeeManagmentSystem.Application.Queries.Employees.GetEmployee;

public sealed record GetEmployeeQuery(Guid Id) : IQuery<EmployeeDto>;

internal sealed class GetEmployeeQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<GetEmployeeQuery, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.Id);

        return EmployeeDto.FromEntity(employee);
    }
}
