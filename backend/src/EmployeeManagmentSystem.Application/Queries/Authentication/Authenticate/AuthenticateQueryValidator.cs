using FluentValidation;

namespace EmployeeManagmentSystem.Application.Queries.Authentication.Authenticate;

internal sealed class AuthenticateQueryValidator : AbstractValidator<AuthenticateQuery>
{
    public AuthenticateQueryValidator()
    {
        RuleFor(query => query.Login)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(login => !string.IsNullOrWhiteSpace(login))
            .WithMessage("'Login' must not be blank.");
        RuleFor(query => query.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(password => !string.IsNullOrWhiteSpace(password))
            .WithMessage("'Password' must not be blank.");
    }
}
