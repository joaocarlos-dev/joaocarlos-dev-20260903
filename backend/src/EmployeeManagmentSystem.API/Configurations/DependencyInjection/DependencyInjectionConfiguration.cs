using EmployeeManagmentSystem.API.Configurations.Jwt;
using EmployeeManagmentSystem.API.Configurations.Swagger;
using EmployeeManagmentSystem.Application;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using EmployeeManagmentSystem.Infrastructure.Services.Employees;
using EmployeeManagmentSystem.Infrastructure.Services.Units;
using EmployeeManagmentSystem.Infrastructure.Services.Users;

namespace EmployeeManagmentSystem.API.Configurations.DependencyInjection;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApiDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
        }

        services.AddControllers();
        services.AddApplicationDependencies();
        services.AddPersistence(connectionString);
        services.AddUserService();
        services.AddEmployeeService();
        services.AddUnitService();
        services.AddJwtConfiguration(configuration);
        services.AddSwaggerConfiguration();

        return services;
    }
}
