using EmployeeManagmentSystem.Infrastructure.Services.Security;

namespace EmployeeManagmentSystem.Tests.UnitTests.Infrastructure.Security;

public sealed class InitialUserOptionsTests
{
    [Fact]
    public void IsConfigured_WithoutPassword_ShouldReturnFalse()
    {
        var options = new InitialUserOptions
        {
            Code = "USR-ADMIN",
            Login = "admin"
        };

        var isConfigured = options.IsConfigured;

        Assert.False(isConfigured);
    }

    [Fact]
    public void Validate_WithWhitespacePassword_ShouldThrowInvalidOperationException()
    {
        var options = new InitialUserOptions
        {
            Code = "USR-ADMIN",
            Login = "admin",
            Password = "        "
        };

        var action = options.Validate;

        Assert.False(options.IsConfigured);
        Assert.Throws<InvalidOperationException>(action);
    }

    [Theory]
    [InlineData("", "admin", "password123")]
    [InlineData("USR-ADMIN", "", "password123")]
    [InlineData("USR-ADMIN", "admin", "short")]
    public void Validate_WithUnsafeConfiguration_ShouldThrowInvalidOperationException(
        string code,
        string login,
        string password)
    {
        var options = new InitialUserOptions
        {
            Code = code,
            Login = login,
            Password = password
        };

        var action = options.Validate;

        Assert.Throws<InvalidOperationException>(action);
    }
}
