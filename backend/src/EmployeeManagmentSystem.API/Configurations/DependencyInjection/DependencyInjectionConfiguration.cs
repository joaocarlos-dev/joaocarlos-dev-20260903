using EmployeeManagmentSystem.API.Configurations.Jwt;
using EmployeeManagmentSystem.API.Configurations.Errors;
using EmployeeManagmentSystem.API.Configurations.Swagger;
using EmployeeManagmentSystem.Application;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using EmployeeManagmentSystem.Infrastructure.Services.Employees;
using EmployeeManagmentSystem.Infrastructure.Services.Units;
using EmployeeManagmentSystem.Infrastructure.Services.Users;
using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Infrastructure.Services.Security;
using EmployeeManagmentSystem.Infrastructure.Services.Outbox;
using EmployeeManagmentSystem.API.Configurations.Outbox;
using EmployeeManagmentSystem.Application.Common.Configuration;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

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
        var loginRateLimitOptions = new LoginRateLimitOptions
        {
            PermitLimit = configuration.GetValue("RateLimiting:Login:PermitLimit", 10),
            WindowSeconds = configuration.GetValue("RateLimiting:Login:WindowSeconds", 60)
        };
        if (loginRateLimitOptions.PermitLimit <= 0 || loginRateLimitOptions.WindowSeconds <= 0)
        {
            throw new InvalidOperationException("RateLimiting:Login must define positive PermitLimit and WindowSeconds.");
        }

        services.AddSingleton(loginRateLimitOptions);
        services.AddScoped<ILoginAttemptLimiter, LoginAttemptLimiter>();
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            if (!configuration.GetValue<bool>("ForwardedHeaders:Enabled"))
            {
                return;
            }

            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var value in configuration.GetSection("ForwardedHeaders:KnownProxies").GetChildren())
            {
                if (!IPAddress.TryParse(value.Value, out var address))
                {
                    throw new InvalidOperationException($"ForwardedHeaders:KnownProxies contains an invalid IP address: {value.Value}");
                }

                options.KnownProxies.Add(address);
            }

            if (options.KnownProxies.Count == 0)
            {
                throw new InvalidOperationException("ForwardedHeaders:KnownProxies is required when forwarding is enabled.");
            }
        });

        services.AddHealthChecks();
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddApplicationDependencies();
        services.AddPersistence(connectionString);
        services.AddUserService();
        services.AddEmployeeService();
        services.AddUnitService();
        services.AddOutboxService();
        var rabbitMqOptions = configuration.GetSection("RabbitMq").Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        if (rabbitMqOptions.Enabled)
        {
            rabbitMqOptions.Validate();
        }

        services.AddSingleton(rabbitMqOptions);
        services.AddHostedService<RabbitMqOutboxPublisher>();
        services.AddJwtConfiguration(configuration);
        services.AddPasswordHasherService();
        services.AddTokenService();
        services.AddSwaggerConfiguration();

        return services;
    }
}
