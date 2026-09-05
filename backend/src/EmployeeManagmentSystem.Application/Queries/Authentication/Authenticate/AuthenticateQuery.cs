using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.DTOs;
using MediatR;

namespace EmployeeManagmentSystem.Application.Queries.Authentication.Authenticate;

public sealed record AuthenticateQuery(string Login, string Password) : IQuery<AuthenticationDto>;

internal sealed class AuthenticateQueryHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService) : IRequestHandler<AuthenticateQuery, AuthenticationDto>
{
    public async Task<AuthenticationDto> Handle(AuthenticateQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByLoginAsync(request.Login, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthenticationException();
        }

        user.EnsureCanAuthenticate();
        return new AuthenticationDto(tokenService.Generate(user));
    }
}
