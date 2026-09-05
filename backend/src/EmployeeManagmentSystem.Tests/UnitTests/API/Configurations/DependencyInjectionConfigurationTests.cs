using EmployeeManagmentSystem.API.Configurations.DependencyInjection;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Commands.Users.CreateUser;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using EmployeeManagmentSystem.Infrastructure.Services.Employees;
using EmployeeManagmentSystem.Infrastructure.Services.Units;
using EmployeeManagmentSystem.Infrastructure.Services.Users;
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
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddApiDependencies_WithEmptyConnectionString_ShouldThrowInvalidOperationException(
        string connectionString)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            })
            .Build();
        var services = new ServiceCollection();

        var action = () => services.AddApiDependencies(configuration);

        var exception = Assert.Throws<InvalidOperationException>(action);
        Assert.Equal("ConnectionStrings:DefaultConnection is required.", exception.Message);
    }

    [Fact]
    public async Task AddApiDependencies_WithValidConfiguration_ShouldRegisterApiDependencies()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Host=localhost;Database=test;Username=test;Password=test",
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
        using var scope = serviceProvider.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var employeeRepository = scope.ServiceProvider.GetRequiredService<IEmployeeRepository>();
        var unitRepository = scope.ServiceProvider.GetRequiredService<IUnitRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        Assert.NotNull(mediator);
        Assert.NotNull(validator);
        Assert.NotNull(bearerScheme);
        Assert.True(hasSwaggerProvider);
        Assert.IsType<UserService>(userRepository);
        Assert.IsType<EmployeeService>(employeeRepository);
        Assert.IsType<UnitService>(unitRepository);
        Assert.IsType<EmployeeManagementDbContext>(unitOfWork);
    }
}
