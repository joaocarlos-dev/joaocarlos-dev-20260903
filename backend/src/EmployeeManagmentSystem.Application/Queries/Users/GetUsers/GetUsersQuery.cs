using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Enums;
using MediatR;

namespace EmployeeManagmentSystem.Application.Queries.Users.GetUsers;

public sealed record GetUsersQuery(EntityStatus? Status) : IQuery<IReadOnlyCollection<UserDto>>;

internal sealed class GetUsersQueryHandler(IUserRepository userRepository)
    : IRequestHandler<GetUsersQuery, IReadOnlyCollection<UserDto>>
{
    public async Task<IReadOnlyCollection<UserDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var users = await userRepository.ListAsync(request.Status, cancellationToken);
        return users.Select(UserDto.FromEntity).ToArray();
    }
}
