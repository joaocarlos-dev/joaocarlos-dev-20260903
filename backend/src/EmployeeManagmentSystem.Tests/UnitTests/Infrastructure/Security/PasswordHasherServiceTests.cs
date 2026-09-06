using EmployeeManagmentSystem.Infrastructure.Services.Security;

namespace EmployeeManagmentSystem.Tests.UnitTests.Infrastructure.Security;

public sealed class PasswordHasherServiceTests
{
    [Fact]
    public void Hash_WithSamePasswordTwice_ShouldCreateDifferentSecureHashes()
    {
        var service = new PasswordHasherService();

        var firstHash = service.Hash("password123");
        var secondHash = service.Hash("password123");

        Assert.NotEqual(firstHash, secondHash);
        Assert.StartsWith("pbkdf2-sha512$210000$", firstHash);
        Assert.True(service.Verify("password123", firstHash));
        Assert.True(service.Verify("password123", secondHash));
    }

    [Fact]
    public void Verify_WithInvalidPassword_ShouldReturnFalse()
    {
        var service = new PasswordHasherService();
        var passwordHash = service.Hash("password123");

        var isValid = service.Verify("wrong-password", passwordHash);

        Assert.False(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid")]
    [InlineData("pbkdf2-sha512$210000$invalid$invalid")]
    [InlineData("pbkdf2-sha512$1$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=")]
    public void Verify_WithMalformedHash_ShouldReturnFalse(string passwordHash)
    {
        var service = new PasswordHasherService();

        var isValid = service.Verify("password123", passwordHash);

        Assert.False(isValid);
    }
}
