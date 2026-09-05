using FluentValidation;

namespace EmployeeManagmentSystem.Application.Queries.Units.GetUnit;

internal sealed class GetUnitQueryValidator : AbstractValidator<GetUnitQuery>
{
    public GetUnitQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
