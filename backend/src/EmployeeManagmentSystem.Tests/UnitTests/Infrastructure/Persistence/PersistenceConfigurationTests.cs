using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EmployeeManagmentSystem.Tests.UnitTests.Infrastructure.Persistence;

public sealed class PersistenceConfigurationTests
{
    [Fact]
    public void Model_WithDomainEntities_ShouldUseExpectedTableNames()
    {
        using var context = CreateContext();

        var userTable = context.Model.FindEntityType(typeof(User))?.GetTableName();
        var employeeTable = context.Model.FindEntityType(typeof(Employee))?.GetTableName();
        var unitTable = context.Model.FindEntityType(typeof(Unit))?.GetTableName();

        Assert.Equal("users", userTable);
        Assert.Equal("employees", employeeTable);
        Assert.Equal("units", unitTable);
    }

    [Fact]
    public void Model_WithUniqueBusinessFields_ShouldCreateUniqueIndexes()
    {
        using var context = CreateContext();
        var userType = context.Model.FindEntityType(typeof(User))!;
        var employeeType = context.Model.FindEntityType(typeof(Employee))!;
        var unitType = context.Model.FindEntityType(typeof(Unit))!;

        var userIndexes = userType.GetIndexes().ToArray();
        var employeeIndexes = employeeType.GetIndexes().ToArray();
        var unitIndexes = unitType.GetIndexes().ToArray();

        Assert.Contains(userIndexes, index => index.IsUnique && HasProperty(index, nameof(User.Code)));
        Assert.Contains(userIndexes, index => index.IsUnique && HasProperty(index, nameof(User.Login)));
        Assert.Contains(employeeIndexes, index => index.IsUnique && HasProperty(index, nameof(Employee.Code)));
        Assert.Contains(employeeIndexes, index => index.IsUnique && HasProperty(index, nameof(Employee.UserId)));
        Assert.Contains(unitIndexes, index => index.IsUnique && HasProperty(index, nameof(Unit.Code)));
    }

    [Fact]
    public void UserModel_ShouldUseCaseInsensitiveLoginColumn()
    {
        using var context = CreateContext();
        var userType = context.Model.FindEntityType(typeof(User))!;

        var loginColumnType = userType.FindProperty(nameof(User.Login))?.GetColumnType();

        Assert.Equal("citext", loginColumnType);
    }

    [Fact]
    public void EmployeeModel_ShouldUseSoftDeleteFilterAndRestrictedRelationships()
    {
        using var context = CreateContext();
        var employeeType = context.Model.FindEntityType(typeof(Employee))!;

        var queryFilter = employeeType.GetQueryFilter();
        var foreignKeys = employeeType.GetForeignKeys().ToArray();

        Assert.NotNull(queryFilter);
        Assert.Equal(2, foreignKeys.Length);
        Assert.All(foreignKeys, foreignKey => Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior));
    }

    [Fact]
    public void Model_WithAuditableEntities_ShouldMapTemporalPropertiesWithTimeZone()
    {
        using var context = CreateContext();
        var entityTypes = new[]
        {
            context.Model.FindEntityType(typeof(User))!,
            context.Model.FindEntityType(typeof(Employee))!,
            context.Model.FindEntityType(typeof(Unit))!
        };

        var temporalColumnTypes = entityTypes
            .SelectMany(entityType => new[]
            {
                entityType.FindProperty(nameof(User.CreatedAt))?.GetColumnType(),
                entityType.FindProperty(nameof(User.UpdatedAt))?.GetColumnType(),
                entityType.FindProperty(nameof(User.DeletedAt))?.GetColumnType()
            })
            .ToArray();

        Assert.All(temporalColumnTypes, columnType => Assert.Equal("timestamp with time zone", columnType));
    }

    private static EmployeeManagementDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<EmployeeManagementDbContext>()
            .UseNpgsql("Host=localhost;Database=model-tests;Username=test;Password=test")
            .Options;

        return new EmployeeManagementDbContext(options);
    }

    private static bool HasProperty(IReadOnlyIndex index, string propertyName) =>
        index.Properties.Count == 1 && index.Properties[0].Name == propertyName;
}
