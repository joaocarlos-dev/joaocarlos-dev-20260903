using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Employees.CreateEmployee;

internal sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty();
        RuleFor(command => command.Name).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.UnitId).NotEmpty();
    }
}
