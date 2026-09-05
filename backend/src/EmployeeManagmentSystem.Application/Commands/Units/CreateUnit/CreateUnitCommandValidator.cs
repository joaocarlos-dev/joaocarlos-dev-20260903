using EmployeeManagmentSystem.Domain.Common;
using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Units.CreateUnit;

internal sealed class CreateUnitCommandValidator : AbstractValidator<CreateUnitCommand>
{
    public CreateUnitCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty()
            .Must(code => code is null || code.Trim().Length <= EntityFieldLengths.Code)
            .WithMessage($"'Code' must be {EntityFieldLengths.Code} characters or fewer.");
        RuleFor(command => command.Name)
            .NotEmpty()
            .Must(name => name is null || name.Trim().Length <= EntityFieldLengths.Name)
            .WithMessage($"'Name' must be {EntityFieldLengths.Name} characters or fewer.");
    }
}
