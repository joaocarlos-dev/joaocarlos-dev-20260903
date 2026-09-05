using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Tests.UnitTests.Domain.Entities;

public sealed class EmployeeTests
{
    [Fact]
    public void Constructor_WithInactiveUnit_ShouldThrowDomainException()
    {
        var unit = new Unit("UNIT-001", "Headquarters");
        unit.ChangeStatus(EntityStatus.Inactive);
        var userId = Guid.NewGuid();

        Action action = () => new Employee("EMP-001", "Employee", userId, unit);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void TransferTo_WithInactiveUnit_ShouldThrowAndKeepCurrentUnit()
    {
        var currentUnit = new Unit("UNIT-001", "Headquarters");
        var inactiveUnit = new Unit("UNIT-002", "Branch");
        inactiveUnit.ChangeStatus(EntityStatus.Inactive);
        var employee = new Employee("EMP-001", "Employee", Guid.NewGuid(), currentUnit);

        Action action = () => employee.TransferTo(inactiveUnit);

        Assert.Throws<DomainException>(action);
        Assert.Equal(currentUnit.Id, employee.UnitId);
    }

    [Fact]
    public void Delete_WithActiveEmployee_ShouldMarkEmployeeAsDeleted()
    {
        var unit = new Unit("UNIT-001", "Headquarters");
        var employee = new Employee("EMP-001", "Employee", Guid.NewGuid(), unit);

        employee.Delete();

        Assert.True(employee.IsDeleted);
        Assert.NotNull(employee.DeletedAt);
        Assert.NotNull(employee.UpdatedAt);
    }
}
