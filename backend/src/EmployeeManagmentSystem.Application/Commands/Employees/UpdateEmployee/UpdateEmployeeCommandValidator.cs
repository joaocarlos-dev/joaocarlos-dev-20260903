using EmployeeManagmentSystem.Domain.Common;
using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Employees.UpdateEmployee;

internal sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name)
            .NotEmpty()
            .Must(name => name is null || name.Trim().Length <= EntityFieldLengths.Name)
            .WithMessage($"'Name' must be {EntityFieldLengths.Name} characters or fewer.")
            .When(command => command.Name is not null);
        RuleFor(command => command.UnitId).NotEmpty().When(command => command.UnitId.HasValue);
        RuleFor(command => command).Must(command => command.Name is not null || command.UnitId.HasValue)
            .WithMessage("At least one employee field must be provided for update.");
    }
}
