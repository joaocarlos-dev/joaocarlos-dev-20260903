using EmployeeManagmentSystem.Domain.Entities;

namespace EmployeeManagmentSystem.Application.Abstractions.Security;

public interface ITokenService
{
    string Generate(User user);
}
