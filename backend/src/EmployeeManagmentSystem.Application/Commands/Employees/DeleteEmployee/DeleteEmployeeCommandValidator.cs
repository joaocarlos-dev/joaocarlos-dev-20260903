using FluentValidation;

namespace EmployeeManagmentSystem.Application.Commands.Employees.DeleteEmployee;

internal sealed class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
