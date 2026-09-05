using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Tests.UnitTests.Domain.Entities;

public sealed class UnitTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateActiveUnit()
    {
        const string code = "UNIT-001";
        const string name = "Headquarters";

        var unit = new Unit(code, name);

        Assert.Equal(EntityStatus.Active, unit.Status);
    }

    [Fact]
    public void EnsureCanReceiveEmployee_WithInactiveUnit_ShouldThrowDomainException()
    {
        var unit = new Unit("UNIT-001", "Headquarters");
        unit.ChangeStatus(EntityStatus.Inactive);

        Action action = unit.EnsureCanReceiveEmployee;

        Assert.Throws<DomainException>(action);
    }
}
