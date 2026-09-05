using EmployeeManagmentSystem.Domain.Entities;

namespace EmployeeManagmentSystem.Application.Abstractions.Persistence;

public interface IUnitRepository
{
    Task<Unit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Unit>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(Unit unit, CancellationToken cancellationToken = default);
}
