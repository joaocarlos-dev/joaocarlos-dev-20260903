using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using MediatR;

namespace EmployeeManagmentSystem.Application.Commands.Employees.UpdateEmployee;

public sealed record UpdateEmployeeCommand(Guid Id, string? Name, Guid? UnitId) : ICommand;

internal sealed class UpdateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IUnitRepository unitRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeCommand>
{
    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.Id);

        if (request.Name is not null)
        {
            employee.UpdateName(request.Name);
        }

        if (request.UnitId.HasValue)
        {
            var unit = await unitRepository.GetByIdAsync(request.UnitId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(EmployeeManagmentSystem.Domain.Entities.Unit), request.UnitId.Value);
            try
            {
                employee.TransferTo(unit);
            }
            catch (DomainException exception)
            {
                throw new ApplicationConflictException(exception.Message);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
