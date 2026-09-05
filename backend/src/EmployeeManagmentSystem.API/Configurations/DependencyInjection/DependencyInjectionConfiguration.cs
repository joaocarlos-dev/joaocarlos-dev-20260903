using EmployeeManagmentSystem.API.Configurations.Jwt;
using EmployeeManagmentSystem.API.Configurations.Swagger;
using EmployeeManagmentSystem.Application;

namespace EmployeeManagmentSystem.API.Configurations.DependencyInjection;

public static class DependencyInjectionConfiguration
{
    public static IServiceCollection AddApiDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddApplicationDependencies();
        services.AddJwtConfiguration(configuration);
        services.AddSwaggerConfiguration();

        return services;
    }
}
