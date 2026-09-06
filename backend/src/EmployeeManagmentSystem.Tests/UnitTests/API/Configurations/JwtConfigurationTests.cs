using EmployeeManagmentSystem.API.Configurations.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagmentSystem.Tests.UnitTests.API.Configurations;

public sealed class JwtConfigurationTests
{
    [Fact]
    public void AddJwtConfiguration_WithSigningKey_ShouldConfigureTokenValidation()
    {
        const string issuer = "test-issuer";
        const string audience = "test-audience";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = issuer,
                ["Jwt:Audience"] = audience,
                ["Jwt:SigningKey"] = "test-signing-key-with-at-least-32-characters",
                ["Jwt:ExpirationMinutes"] = "30"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddJwtConfiguration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal(issuer, options.TokenValidationParameters.ValidIssuer);
        Assert.Equal(audience, options.TokenValidationParameters.ValidAudience);
        Assert.IsType<SymmetricSecurityKey>(options.TokenValidationParameters.IssuerSigningKey);
        Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);
        Assert.True(options.TokenValidationParameters.ValidateLifetime);
        Assert.Equal(TimeSpan.Zero, options.TokenValidationParameters.ClockSkew);
    }

    [Theory]
    [InlineData("Jwt:Issuer")]
    [InlineData("Jwt:Audience")]
    [InlineData("Jwt:SigningKey")]
    public void AddJwtConfiguration_WithMissingRequiredSetting_ShouldThrowInvalidOperationException(string missingSetting)
    {
        var settings = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:SigningKey"] = "test-signing-key-with-at-least-32-characters",
            ["Jwt:ExpirationMinutes"] = "30"
        };
        settings[missingSetting] = null;
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
        var services = new ServiceCollection();

        var action = () => services.AddJwtConfiguration(configuration);

        Assert.Throws<InvalidOperationException>(action);
    }

    [Theory]
    [InlineData("short-key", 30)]
    [InlineData("test-signing-key-with-at-least-32-characters", 0)]
    [InlineData("test-signing-key-with-at-least-32-characters", 1441)]
    public void AddJwtConfiguration_WithUnsafeSetting_ShouldThrowInvalidOperationException(
        string signingKey,
        int expirationMinutes)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:SigningKey"] = signingKey,
                ["Jwt:ExpirationMinutes"] = expirationMinutes.ToString()
            })
            .Build();
        var services = new ServiceCollection();

        var action = () => services.AddJwtConfiguration(configuration);

        Assert.Throws<InvalidOperationException>(action);
    }
}
