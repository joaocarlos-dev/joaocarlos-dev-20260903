using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Infrastructure.Persistence;

public static class PersistenceDependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<EmployeeManagementDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<EmployeeManagementDbContext>());

        return services;
    }
}
