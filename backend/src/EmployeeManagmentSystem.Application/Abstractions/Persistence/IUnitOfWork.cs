namespace EmployeeManagmentSystem.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<IUnitOfWorkTransaction> BeginSerializableTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
