using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Domain.Enums;
using EmployeeManagmentSystem.Infrastructure.Services.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagmentSystem.Tests.UnitTests.Infrastructure.Security;

public sealed class TokenServiceTests
{
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";
    private const string SigningKey = "test-signing-key-with-at-least-32-characters";

    [Fact]
    public void Generate_WithValidUser_ShouldCreateValidTokenWithIdentityClaims()
    {
        var service = CreateService();
        var user = new User("USR-001", "admin", "password-hash", EntityStatus.Active, UserRole.Administrator);

        var encodedToken = service.Generate(user);

        var principal = new JwtSecurityTokenHandler().ValidateToken(
            encodedToken,
            CreateValidationParameters(),
            out var validatedToken);
        Assert.IsType<JwtSecurityToken>(validatedToken);
        Assert.Equal(user.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(user.Login, principal.Identity?.Name);
        Assert.Equal(UserRole.Administrator.ToString(), principal.FindFirstValue(ClaimTypes.Role));
        Assert.Equal("0", principal.FindFirstValue(TokenClaims.SecurityVersion));
    }

    [Fact]
    public void Generate_WithDifferentSigningKey_ShouldFailValidation()
    {
        var service = CreateService();
        var user = new User("USR-001", "admin", "password-hash");
        var encodedToken = service.Generate(user);
        var validationParameters = CreateValidationParameters();
        validationParameters.IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("different-signing-key-with-at-least-32-characters"));

        var action = () => new JwtSecurityTokenHandler().ValidateToken(
            encodedToken,
            validationParameters,
            out _);

        Assert.ThrowsAny<SecurityTokenException>(action);
    }

    [Fact]
    public void Constructor_WithUnsafeConfiguration_ShouldThrowInvalidOperationException()
    {
        var options = Options.Create(new JwtOptions
        {
            Issuer = Issuer,
            Audience = Audience,
            SigningKey = "short",
            ExpirationMinutes = 30
        });

        var action = () => new TokenService(options);

        Assert.Throws<InvalidOperationException>(action);
    }

    private static TokenService CreateService() =>
        new(Options.Create(new JwtOptions
        {
            Issuer = Issuer,
            Audience = Audience,
            SigningKey = SigningKey,
            ExpirationMinutes = 30
        }));

    private static TokenValidationParameters CreateValidationParameters() =>
        new()
        {
            ValidateIssuer = true,
            ValidIssuer = Issuer,
            ValidateAudience = true,
            ValidAudience = Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
}
