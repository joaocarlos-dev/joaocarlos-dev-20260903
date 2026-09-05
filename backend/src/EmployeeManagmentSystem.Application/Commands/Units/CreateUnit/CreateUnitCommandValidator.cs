using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Units.CreateUnit;

internal sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty();
        RuleFor(command => command.Name).NotEmpty();
    }
}
