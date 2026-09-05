using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Domain.Entities;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Infrastructure.Services.Employees;

public sealed class EmployeeService(EmployeeManagementDbContext context) : IEmployeeRepository
{
    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Employees.SingleOrDefaultAsync(employee => employee.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Employee>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Employees.OrderBy(employee => employee.Name).ToArrayAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Employee>> ListByUnitIdsAsync(
        IReadOnlyCollection<Guid> unitIds,
        CancellationToken cancellationToken = default) =>
        await context.Employees
            .Where(employee => unitIds.Contains(employee.UnitId))
            .OrderBy(employee => employee.Name)
            .ToArrayAsync(cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        context.Employees
            .IgnoreQueryFilters()
            .AnyAsync(employee => employee.Code == code, cancellationToken);

    public Task<bool> UserIsLinkedAsync(Guid userId, CancellationToken cancellationToken = default) =>
        context.Employees
            .IgnoreQueryFilters()
            .AnyAsync(employee => employee.UserId == userId, cancellationToken);

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        await context.Employees.AddAsync(employee, cancellationToken);
    }
}

public static class EmployeeServiceDependencyInjection
{
    public static IServiceCollection AddEmployeeService(this IServiceCollection services)
    {
        services.AddScoped<IEmployeeRepository, EmployeeService>();
        return services;
    }
}
