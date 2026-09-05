namespace EmployeeManagmentSystem.API.Configurations;

public static class ApiConfiguration
{
    public static WebApplication UseApiConfiguration(this WebApplication app)
    {
        app.UseSwaggerConfiguration();
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
