using System.Text;
using EmployeeManagmentSystem.Infrastructure.Services.Security;
using EmployeeManagmentSystem.Domain.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace EmployeeManagmentSystem.API.Configurations.Jwt;

public static class JwtConfiguration
{
    public static IServiceCollection AddJwtConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = new JwtOptions
        {
            Issuer = configuration["Jwt:Issuer"] ?? string.Empty,
            Audience = configuration["Jwt:Audience"] ?? string.Empty,
            SigningKey = configuration["Jwt:SigningKey"] ?? string.Empty,
            ExpirationMinutes = configuration.GetValue<int?>("Jwt:ExpirationMinutes") ?? 60
        };
        jwtOptions.Validate();
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

        services.AddSingleton(Options.Create(jwtOptions));

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();

                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        await context.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Status = StatusCodes.Status401Unauthorized,
                            Title = "Unauthorized",
                            Detail = "Authentication credentials are missing or invalid.",
                            Instance = context.Request.Path
                        });
                    }
                };
            });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicies.AdministratorOnly,
                policy => policy.RequireRole(UserRole.Administrator.ToString()));
        });

        return services;
    }
}
