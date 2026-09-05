using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Domain.Entities;
using MediatR;

namespace EmployeeManagmentSystem.Application.Commands.Employees.DeleteEmployee;

public sealed record DeleteEmployeeCommand(Guid Id) : ICommand;

internal sealed class DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.Id);

        employee.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
