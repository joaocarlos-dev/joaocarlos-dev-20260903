using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Units.UpdateUnit;

internal sealed class UpdateUnitCommandValidator : AbstractValidator<UpdateUnitCommand>
{
    public UpdateUnitCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().When(command => command.Name is not null);
        RuleFor(command => command.Status).IsInEnum().When(command => command.Status.HasValue);
        RuleFor(command => command).Must(command => command.Name is not null || command.Status.HasValue)
            .WithMessage("At least one unit field must be provided for update.");
    }
}
