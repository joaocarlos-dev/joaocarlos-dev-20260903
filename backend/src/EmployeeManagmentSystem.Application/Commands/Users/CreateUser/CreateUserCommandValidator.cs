using EmployeeManagmentSystem.Domain.Common;
using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Users.CreateUser;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty()
            .Must(code => code is null || code.Trim().Length <= EntityFieldLengths.Code)
            .WithMessage($"'Code' must be {EntityFieldLengths.Code} characters or fewer.");
        RuleFor(command => command.Login)
            .NotEmpty()
            .Must(login => login is null || login.Trim().Length <= EntityFieldLengths.Login)
            .WithMessage($"'Login' must be {EntityFieldLengths.Login} characters or fewer.");
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8);
        RuleFor(command => command.Status).IsInEnum();
    }
}
