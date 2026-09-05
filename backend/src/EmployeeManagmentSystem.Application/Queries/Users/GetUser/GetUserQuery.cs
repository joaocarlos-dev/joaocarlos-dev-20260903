using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Entities;
using MediatR;

namespace EmployeeManagmentSystem.Application.Queries.Users.GetUser;

public sealed record GetUserQuery(Guid Id) : IQuery<UserDto>;

internal sealed class GetUserQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.Id);

        return UserDto.FromEntity(user);
    }
}
