using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Domain.Entities;
using MediatR;

namespace EmployeeManagmentSystem.Application.Commands.Employees.CreateEmployee;

public sealed record CreateEmployeeCommand(string Code, string Name, Guid UserId, Guid UnitId) : ICommand<Guid>;

internal sealed class CreateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IUnitRepository unitRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEmployeeCommand, Guid>
{
    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();

        if (await employeeRepository.CodeExistsAsync(code, cancellationToken))
        {
            throw new ApplicationConflictException("The employee code is already in use.");
        }

        if (await employeeRepository.UserIsLinkedAsync(request.UserId, cancellationToken))
        {
            throw new ApplicationConflictException("The user is already linked to an employee.");
        }

        _ = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);
        var unit = await unitRepository.GetByIdAsync(request.UnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(EmployeeManagmentSystem.Domain.Entities.Unit), request.UnitId);

        var employee = new Employee(code, request.Name, request.UserId, unit);
        await employeeRepository.AddAsync(employee, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}
