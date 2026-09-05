using FluentValidation;

namespace EmployeeManagmentSystem.Application.Queries.Users.GetUsers;

internal sealed class GetUsersQueryValidator : AbstractValidator<GetUsersQuery>
{
    public GetUsersQueryValidator()
    {
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}
