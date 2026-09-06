using EmployeeManagmentSystem.API.Configurations.Swagger;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using EmployeeManagmentSystem.Infrastructure.Services.Security;

namespace EmployeeManagmentSystem.API.Configurations.Pipeline;

public static class ApiConfiguration
{
    public static async Task UseApiConfigurationAsync(
        this WebApplication app,
        IConfiguration configuration)
    {
        var initialUser = new InitialUserOptions
        {
            Code = configuration["InitialUser:Code"] ?? string.Empty,
            Login = configuration["InitialUser:Login"] ?? string.Empty,
            Password = configuration["InitialUser:Password"]
        };
        await app.Services.InitializeDatabaseAsync(initialUser);

        app.UseSwaggerConfiguration();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
    }
}
