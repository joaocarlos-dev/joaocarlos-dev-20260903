using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Domain.Common;
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
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var code = request.Code.Trim();

            var user = await userRepository.GetByIdForUpdateAsync(request.UserId, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.UserId);
            if (!user.IsActive)
            {
                throw new ApplicationConflictException("An inactive user cannot be linked to an employee.");
            }

            if (await employeeRepository.UserIsLinkedAsync(request.UserId, cancellationToken))
            {
                throw new ApplicationConflictException("The user is already linked to an employee.");
            }

            if (await employeeRepository.CodeExistsAsync(code, cancellationToken))
            {
                throw new ApplicationConflictException("The employee code is already in use.");
            }
            var unit = await unitRepository.GetByIdAsync(request.UnitId, cancellationToken)
                ?? throw new NotFoundException(nameof(EmployeeManagmentSystem.Domain.Entities.Unit), request.UnitId);

            Employee employee;
            try
            {
                employee = new Employee(code, request.Name, request.UserId, unit);
            }
            catch (DomainException exception)
            {
                throw new ApplicationConflictException(exception.Message);
            }

            await employeeRepository.AddAsync(employee, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return employee.Id;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}
