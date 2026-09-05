using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagmentSystem.API.Configurations;

public static class JwtConfiguration
{
    public static IServiceCollection AddJwtConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var signingKey = configuration["Jwt:SigningKey"];

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = CreateSigningKey(signingKey),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });
        services.AddAuthorization();

        return services;
    }

    private static SymmetricSecurityKey? CreateSigningKey(string? signingKey)
    {
        return string.IsNullOrWhiteSpace(signingKey)
            ? null
            : new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    }
}
