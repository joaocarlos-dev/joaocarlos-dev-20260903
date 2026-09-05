using EmployeeManagmentSystem.API.Configurations.DependencyInjection;
using EmployeeManagmentSystem.Application.Commands.Users.CreateUser;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace EmployeeManagmentSystem.Tests.UnitTests.API.Configurations;

public sealed class DependencyInjectionConfigurationTests
{
    [Fact]
    public async Task AddApiDependencies_WithValidConfiguration_ShouldRegisterApiDependencies()
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
        var validator = serviceProvider.GetService<IValidator<CreateUserCommand>>();
        var schemeProvider = serviceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var bearerScheme = await schemeProvider.GetSchemeAsync(JwtBearerDefaults.AuthenticationScheme);
        var hasSwaggerProvider = services.Any(service => service.ServiceType == typeof(ISwaggerProvider));

        Assert.NotNull(mediator);
        Assert.NotNull(validator);
        Assert.NotNull(bearerScheme);
        Assert.True(hasSwaggerProvider);
    }
}
