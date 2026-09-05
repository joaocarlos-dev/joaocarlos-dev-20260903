using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DomainUnit = EmployeeManagmentSystem.Domain.Entities.Unit;

namespace EmployeeManagmentSystem.Infrastructure.Services.Units;

public sealed class UnitService(EmployeeManagementDbContext context) : IUnitRepository
{
    public Task<DomainUnit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        context.Units.SingleOrDefaultAsync(unit => unit.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<DomainUnit>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Units.OrderBy(unit => unit.Name).ToArrayAsync(cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
        context.Units.AnyAsync(unit => unit.Code == code, cancellationToken);

    public async Task AddAsync(DomainUnit unit, CancellationToken cancellationToken = default)
    {
        await context.Units.AddAsync(unit, cancellationToken);
    }
}

public static class UnitServiceDependencyInjection
{
    public static IServiceCollection AddUnitService(this IServiceCollection services)
    {
        services.AddScoped<IUnitRepository, UnitService>();
        return services;
    }
}
