using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using MediatR;

namespace EmployeeManagmentSystem.Application.Commands.Users.UpdateUser;

public sealed record UpdateUserCommand(Guid Id, string? Password, EntityStatus? Status) : ICommand;

internal sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginSerializableTransactionAsync(cancellationToken);

        try
        {
            var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(User), request.Id);

            if (request.Password is not null)
            {
                user.UpdatePassword(passwordHasher.Hash(request.Password));
            }

            if (request.Status.HasValue)
            {
                if (user.IsAdministrator && request.Status.Value == EntityStatus.Inactive
                    && await userRepository.CountAdministratorsAsync(cancellationToken) <= 1)
                {
                    throw new ApplicationConflictException("The last administrator cannot be deactivated.");
                }

                user.ChangeStatus(request.Status.Value);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }

    }
}
