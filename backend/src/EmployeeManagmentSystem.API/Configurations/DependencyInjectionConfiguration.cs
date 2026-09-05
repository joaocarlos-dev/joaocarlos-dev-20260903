using EmployeeManagmentSystem.Application;

namespace EmployeeManagmentSystem.API.Configurations;

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
