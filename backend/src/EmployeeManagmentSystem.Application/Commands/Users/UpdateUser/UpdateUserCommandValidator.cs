using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Users.UpdateUser;

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
