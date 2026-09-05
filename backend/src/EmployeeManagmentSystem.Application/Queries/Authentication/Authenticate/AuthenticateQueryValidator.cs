using FluentValidation;

namespace EmployeeManagmentSystem.Application.Queries.Authentication.Authenticate;

internal sealed class AuthenticateQueryValidator : AbstractValidator<AuthenticateQuery>
{
    public AuthenticateQueryValidator()
    {
        RuleFor(query => query.Login).NotEmpty();
        RuleFor(query => query.Password).NotEmpty();
    }
}
