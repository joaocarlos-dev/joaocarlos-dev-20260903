using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EmployeeManagmentSystem.Infrastructure.Persistence;

public sealed class EmployeeManagementDbContextFactory : IDesignTimeDbContextFactory<EmployeeManagementDbContext>
{
    public EmployeeManagementDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings__DefaultConnection is required for migrations.");
        }

        var options = new DbContextOptionsBuilder<EmployeeManagementDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new EmployeeManagementDbContext(options);
    }
}
