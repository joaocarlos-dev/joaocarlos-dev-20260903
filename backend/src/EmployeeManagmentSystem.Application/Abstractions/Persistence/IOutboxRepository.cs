using EmployeeManagmentSystem.Application.Common.Events;

namespace EmployeeManagmentSystem.Application.Abstractions.Persistence;

public interface IOutboxRepository
{
    string WorkerId { get; }
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken cancellationToken = default);
    Task<bool> MarkProcessedAsync(Guid id, string workerId, CancellationToken cancellationToken = default);
    Task<bool> MarkFailedAsync(Guid id, string error, string workerId, CancellationToken cancellationToken = default);
    Task PruneProcessedAsync(TimeSpan retention, int batchSize, CancellationToken cancellationToken = default);
}
