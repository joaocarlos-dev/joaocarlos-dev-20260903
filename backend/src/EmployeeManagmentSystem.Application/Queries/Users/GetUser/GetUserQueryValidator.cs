using FluentValidation;

namespace EmployeeManagmentSystem.Application.Queries.Users.GetUser;

internal sealed class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}
