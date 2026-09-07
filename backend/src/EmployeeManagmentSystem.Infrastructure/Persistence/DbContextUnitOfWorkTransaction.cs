using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace EmployeeManagmentSystem.Infrastructure.Persistence;

internal sealed class DbContextUnitOfWorkTransaction(IDbContextTransaction transaction) : IUnitOfWorkTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default) => transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default) => transaction.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
