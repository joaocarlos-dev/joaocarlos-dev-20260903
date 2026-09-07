using EmployeeManagmentSystem.Domain.Common;
using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Users.CreateUser;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(code => !string.IsNullOrWhiteSpace(code))
            .WithMessage("'Code' must not be blank.")
            .Must(code => code is null || code.Trim().Length <= EntityFieldLengths.Code)
            .WithMessage($"'Code' must be {EntityFieldLengths.Code} characters or fewer.");
        RuleFor(command => command.Login)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(login => !string.IsNullOrWhiteSpace(login))
            .WithMessage("'Login' must not be blank.")
            .Must(login => login is null || login.Trim().Length <= EntityFieldLengths.Login)
            .WithMessage($"'Login' must be {EntityFieldLengths.Login} characters or fewer.");
        RuleFor(command => command.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(password => !string.IsNullOrWhiteSpace(password))
            .WithMessage("'Password' must not be blank.")
            .MinimumLength(8);
        RuleFor(command => command.Status).IsInEnum();
        RuleFor(command => command.Role).IsInEnum();
    }
}
