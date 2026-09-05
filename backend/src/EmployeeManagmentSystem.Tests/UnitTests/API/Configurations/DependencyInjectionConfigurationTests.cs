using EmployeeManagmentSystem.API.Configurations;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Tests.UnitTests.API.Configurations;

public sealed class DependencyInjectionConfigurationTests
{
    [Fact]
    public async Task AddApiDependencies_WithValidConfiguration_ShouldRegisterApplicationAndAuthentication()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:SigningKey"] = "test-signing-key-with-at-least-32-characters"
            })
            .Build();
        var services = new ServiceCollection();

        services.AddApiDependencies(configuration);
        using var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetService<IMediator>();
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var bearerScheme = await schemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);

        Assert.NotNull(mediator);
        Assert.NotNull(bearerScheme);
    }
}
