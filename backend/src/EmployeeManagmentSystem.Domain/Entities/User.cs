using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Domain.Entities;

public sealed class User : AuditableEntity
{
    private User()
    {
    }

    public User(string code, string login, string passwordHash)
        : this(code, login, passwordHash, EntityStatus.Active, UserRole.Conventional)
    {
    }

    public User(string code, string login, string passwordHash, EntityStatus status)
        : this(code, login, passwordHash, status, UserRole.Conventional)
    {
    }

    public User(
        string code,
        string login,
        string passwordHash,
        EntityStatus status,
        UserRole role)
        : base(Guid.NewGuid())
    {
        Code = DomainRules.Required(code, nameof(code));
        Login = DomainRules.Required(login, nameof(login));
        PasswordHash = DomainRules.Required(passwordHash, nameof(passwordHash));
        Status = EnsureValidStatus(status);
        Role = EnsureValidRole(role);
    }

    public string Code { get; private set; } = string.Empty;
    public string Login { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public EntityStatus Status { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive => Status == EntityStatus.Active;
    public bool IsAdministrator => Role == UserRole.Administrator;

    public void UpdatePassword(string passwordHash)
    {
        PasswordHash = DomainRules.Required(passwordHash, nameof(passwordHash));
        MarkAsUpdated();
    }

    public void ChangeStatus(EntityStatus status)
    {
        Status = EnsureValidStatus(status);
        MarkAsUpdated();
    }

    public void ChangeRole(UserRole role)
    {
        Role = EnsureValidRole(role);
        MarkAsUpdated();
    }

    public void EnsureCanAuthenticate()
    {
        if (!IsActive)
        {
            throw new DomainException("An inactive user cannot authenticate.");
        }
    }

    private static EntityStatus EnsureValidStatus(EntityStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException("The user status is invalid.");
        }

        return status;
    }

    private static UserRole EnsureValidRole(UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new DomainException("The user role is invalid.");
        }

        return role;
    }
}
