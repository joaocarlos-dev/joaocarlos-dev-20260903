using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using FluentValidation;
using MediatR;

namespace EmployeeManagmentSystem.Application.Users;

public sealed record CreateUserCommand(string Code, string Login, string Password) : ICommand<Guid>;

public sealed record UpdateUserCommand(Guid Id, string? Password, EntityStatus? Status) : ICommand;

public sealed record GetUserQuery(Guid Id) : IQuery<UserDto>;

public sealed record ListUsersQuery(EntityStatus? Status) : IQuery<IReadOnlyCollection<UserDto>>;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty();
        RuleFor(command => command.Login).NotEmpty();
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8);
    }
}

internal sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Password).MinimumLength(8).When(command => command.Password is not null);
        RuleFor(command => command.Status).IsInEnum().When(command => command.Status.HasValue);
        RuleFor(command => command).Must(command => command.Password is not null || command.Status.HasValue)
            .WithMessage("At least one user field must be provided for update.");
    }
}

internal sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}

internal sealed class ListUsersQueryValidator : AbstractValidator<ListUsersQuery>
{
    public ListUsersQueryValidator()
    {
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}

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

        var user = new User(code, login, passwordHasher.Hash(request.Password));
        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}

internal sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        if (request.Password is not null)
        {
            user.UpdatePassword(passwordHasher.Hash(request.Password));
        }

        if (request.Status.HasValue)
        {
            user.ChangeStatus(request.Status.Value);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class GetUserQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        return UserDto.FromEntity(user);
    }
}

internal sealed class ListUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<ListUsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(
        ListUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await userRepository.ListAsync(request.Status, cancellationToken);
        return users.Select(UserDto.FromEntity).ToArray();
    }
}
