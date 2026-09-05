using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using MediatR;
using DomainUnit = EmployeeManagmentSystem.Domain.Entities.Unit;

namespace EmployeeManagmentSystem.Application.Commands.Units.CreateUnit;

public sealed record CreateUnitCommand(string Code, string Name) : ICommand<Guid>;

internal sealed class CreateUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateUnitCommand, Guid>
{
    public async Task<Guid> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();

        if (await unitRepository.CodeExistsAsync(code, cancellationToken))
        {
            throw new ApplicationConflictException("The unit code is already in use.");
        }

        var unit = new DomainUnit(code, request.Name);
        await unitRepository.AddAsync(unit, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return unit.Id;
    }
}
