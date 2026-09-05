using EmployeeManagmentSystem.Domain.Entities;

namespace EmployeeManagmentSystem.Application.Abstractions.Persistence;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Employee>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Employee>> ListByUnitIdsAsync(
        IReadOnlyCollection<Guid> unitIds,
        CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task<bool> UserIsLinkedAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Employee employee, CancellationToken cancellationToken = default);
}
