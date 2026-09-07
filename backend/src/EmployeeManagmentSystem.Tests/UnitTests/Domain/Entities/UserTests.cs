using EmployeeManagmentSystem.Domain.Common;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;

namespace EmployeeManagmentSystem.Tests.UnitTests.Domain.Entities;

public sealed class UserTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateActiveUser()
    {
        const string code = "USR-001";
        const string login = "admin";
        const string passwordHash = "hashed-password";

        var user = new User(code, login, passwordHash);

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal(EntityStatus.Active, user.Status);
        Assert.False(user.IsDeleted);
        Assert.Null(user.UpdatedAt);
        Assert.Equal(UserRole.Conventional, user.Role);
    }

    [Fact]
    public void Constructor_WithAdministratorRole_ShouldCreateAdministrator()
    {
        var user = new User("USR-001", "admin", "hashed-password", EntityStatus.Active, UserRole.Administrator);

        Assert.True(user.IsAdministrator);
        Assert.Equal(UserRole.Administrator, user.Role);
    }

    [Fact]
    public void Constructor_WithInvalidRole_ShouldThrowDomainException()
    {
        var invalidRole = (UserRole)999;

        Action action = () => new User("USR-001", "admin", "hashed-password", EntityStatus.Active, invalidRole);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void Constructor_WithInactiveStatus_ShouldCreateInactiveUser()
    {
        const EntityStatus status = EntityStatus.Inactive;

        var user = new User("USR-001", "admin", "hashed-password", status);

        Assert.Equal(status, user.Status);
        Assert.False(user.IsActive);
        Assert.Null(user.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithInvalidStatus_ShouldThrowDomainException()
    {
        var invalidStatus = (EntityStatus)999;

        Action action = () => new User("USR-001", "admin", "hashed-password", invalidStatus);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void UpdatePassword_WithEmptyHash_ShouldThrowDomainException()
    {
        var user = new User("USR-001", "admin", "hashed-password");
        const string invalidPasswordHash = " ";

        Action action = () => user.UpdatePassword(invalidPasswordHash);

        Assert.Throws<DomainException>(action);
    }

    [Fact]
    public void UpdatePassword_WithValidHash_ShouldUpdatePasswordAndAuditDate()
    {
        var user = new User("USR-001", "admin", "hashed-password");
        const string newPasswordHash = "new-hashed-password";

        user.UpdatePassword(newPasswordHash);

        Assert.Equal(newPasswordHash, user.PasswordHash);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void ChangeStatus_WithInactiveStatus_ShouldUpdateUser()
    {
        var user = new User("USR-001", "admin", "hashed-password");

        user.ChangeStatus(EntityStatus.Inactive);

        Assert.Equal(EntityStatus.Inactive, user.Status);
        Assert.NotNull(user.UpdatedAt);
    }

    [Fact]
    public void EnsureCanAuthenticate_WithInactiveUser_ShouldThrowDomainException()
    {
        var user = new User("USR-001", "admin", "hashed-password");
        user.ChangeStatus(EntityStatus.Inactive);

        Action action = user.EnsureCanAuthenticate;

        Assert.Throws<DomainException>(action);
    }
}
