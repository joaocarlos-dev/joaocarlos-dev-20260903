using FluentValidation;

namespace EmployeeManagmentSystem.Application.Queries.Employees.GetEmployee;

internal sealed class GetEmployeeQueryValidator : AbstractValidator<GetEmployeeQuery>
{
    public GetEmployeeQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
