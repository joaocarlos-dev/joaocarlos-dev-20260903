using EmployeeManagmentSystem.API.Configurations;
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
                ["Jwt:SigningKey"] = "test-signing-key-with-at-least-32-characters"
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

    [Fact]
    public void AddJwtConfiguration_WithoutSigningKey_ShouldKeepValidationFailClosed()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddJwtConfiguration(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider
            .GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
            .Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);
        Assert.Null(options.TokenValidationParameters.IssuerSigningKey);
    }
}
