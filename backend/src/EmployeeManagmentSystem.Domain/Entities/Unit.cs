using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Domain.Entities;

public sealed class Unit : AuditableEntity
{
    private Unit()
    {
    }

    public Unit(string code, string name)
        : base(Guid.NewGuid())
    {
        Code = DomainRules.Required(code, nameof(code));
        Name = DomainRules.Required(name, nameof(name));
        Status = EntityStatus.Active;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public EntityStatus Status { get; private set; }
    public bool IsActive => Status == EntityStatus.Active;

    public void UpdateName(string name)
    {
        Name = DomainRules.Required(name, nameof(name));
        MarkAsUpdated();
    }

    public void ChangeStatus(EntityStatus status)
    {
        if (!Enum.IsDefined(status))
        {
            throw new DomainException("The unit status is invalid.");
        }

        Status = status;
        MarkAsUpdated();
    }

    public void EnsureCanReceiveEmployee()
    {
        if (!IsActive)
        {
            throw new DomainException("An inactive unit cannot receive employees.");
        }
    }
}
