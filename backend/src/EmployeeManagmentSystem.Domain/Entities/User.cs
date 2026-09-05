using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Domain.Entities;

public sealed class User : AuditableEntity
{
    private User()
    {
    }

    public User(string code, string login, string passwordHash)
        : base(Guid.NewGuid())
    {
        Code = DomainRules.Required(code, nameof(code));
        Login = DomainRules.Required(login, nameof(login));
        PasswordHash = DomainRules.Required(passwordHash, nameof(passwordHash));
        Status = EntityStatus.Active;
    }

    public string Code { get; private set; } = string.Empty;
    public string Login { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public EntityStatus Status { get; private set; }

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = DomainRules.Required(passwordHash, nameof(passwordHash));
        MarkAsUpdated();
    }

    public void ChangeStatus(EntityStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException("The user status is invalid.");
        }

        Status = status;
        MarkAsUpdated();
    }
}
