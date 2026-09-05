using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using MediatR;

namespace EmployeeManagmentSystem.Application.Commands.Users.CreateUser;

public sealed record CreateUserCommand(string Code, string Login, string Password, EntityStatus Status) : ICommand<Guid>;

internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();
        var login = request.Login.Trim();

        if (await userRepository.CodeExistsAsync(code, cancellationToken))
        {
            throw new ApplicationConflictException("The user code is already in use.");
        }

        if (await userRepository.LoginExistsAsync(login, cancellationToken))
        {
            throw new ApplicationConflictException("The user login is already in use.");
        }

        var user = new User(code, login, passwordHasher.Hash(request.Password), request.Status);
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
