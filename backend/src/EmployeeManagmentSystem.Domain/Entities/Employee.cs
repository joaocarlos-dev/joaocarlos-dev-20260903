using EmployeeManagmentSystem.Domain.Common;

namespace EmployeeManagmentSystem.Domain.Entities;

public sealed class Employee : AuditableEntity
{
    private Employee()
    {
    }

    public Employee(string code, string name, Guid userId, Unit unit)
        : base(Guid.NewGuid())
    {
        ArgumentNullException.ThrowIfNull(unit);
        unit.EnsureCanReceiveEmployee();

        Code = DomainRules.Required(code, nameof(code));
        Name = DomainRules.Required(name, nameof(name));
        UserId = DomainRules.Required(userId, nameof(userId));
        UnitId = unit.Id;
    }

    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public Guid UnitId { get; private set; }

    public void UpdateName(string name)
    {
        EnsureNotDeleted("employee");
        Name = DomainRules.Required(name, nameof(name));
        MarkAsUpdated();
    }

    public void TransferTo(Unit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        EnsureNotDeleted("employee");
        unit.EnsureCanReceiveEmployee();

        UnitId = unit.Id;
        MarkAsUpdated();
    }

    public void Delete()
    {
        MarkAsDeleted();
    }
}
