using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Domain.Enums;
using MediatR;
using DomainUnit = EmployeeManagmentSystem.Domain.Entities.Unit;

namespace EmployeeManagmentSystem.Application.Commands.Units.UpdateUnit;

public sealed record UpdateUnitCommand(Guid Id, string? Name, EntityStatus? Status) : ICommand;

internal sealed class UpdateUnitCommandHandler(IUnitRepository unitRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateUnitCommand>
{
    public async Task Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await unitRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DomainUnit), request.Id);

        if (request.Name is not null)
        {
            unit.UpdateName(request.Name);
        }

        if (request.Status.HasValue)
        {
            unit.ChangeStatus(request.Status.Value);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
