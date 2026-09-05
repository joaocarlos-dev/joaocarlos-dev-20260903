using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Users.CreateUser;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty();
        RuleFor(command => command.Login).NotEmpty();
        RuleFor(command => command.Password).NotEmpty().MinimumLength(8);
    }
}
