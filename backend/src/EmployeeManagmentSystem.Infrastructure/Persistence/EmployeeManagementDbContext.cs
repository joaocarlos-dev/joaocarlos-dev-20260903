using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagmentSystem.Infrastructure.Persistence;

public sealed class EmployeeManagementDbContext(DbContextOptions<EmployeeManagementDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Unit> Units => Set<Unit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EmployeeManagementDbContext).Assembly);
    }
}
