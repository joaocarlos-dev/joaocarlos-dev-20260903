using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Application.DTOs;

public sealed record UserDto(
    Guid Id,
    string Code,
    string Login,
    EntityStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt)
{
    public static UserDto FromEntity(User user) =>
        new(user.Id, user.Code, user.Login, user.Status, user.CreatedAt, user.UpdatedAt);
}
