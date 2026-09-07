using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Events;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagmentSystem.Infrastructure.Services.Outbox;

public sealed class OutboxService(EmployeeManagementDbContext context) : IOutboxRepository
{
    public string WorkerId { get; } = Guid.NewGuid().ToString("N");
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken = default) =>
        await context.Set<OutboxMessage>().AddAsync(message, cancellationToken);

    public async Task<IReadOnlyCollection<OutboxMessage>> GetPendingAsync(
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        try
        {
            var messages = await context.OutboxMessages
                .FromSqlInterpolated($"SELECT * FROM outbox_messages WHERE processed_at IS NULL AND dead_lettered_at IS NULL AND (next_attempt_at IS NULL OR next_attempt_at <= {now}) AND (claimed_until IS NULL OR claimed_until <= {now}) ORDER BY occurred_at LIMIT {batchSize} FOR UPDATE SKIP LOCKED")
                .OrderBy(message => message.OccurredAt)
                .ToArrayAsync(cancellationToken);
            foreach (var message in messages)
            {
                message.ClaimedBy = WorkerId;
                message.ClaimedUntil = now.AddMinutes(30);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return messages;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    public async Task<bool> MarkProcessedAsync(Guid id, string owner, CancellationToken cancellationToken = default)
    {
        var affectedRows = await context.OutboxMessages
            .Where(message => message.Id == id && message.ClaimedBy == owner && message.ProcessedAt == null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(message => message.ProcessedAt, DateTimeOffset.UtcNow)
                .SetProperty(message => message.ClaimedBy, (string?)null)
                .SetProperty(message => message.ClaimedUntil, (DateTimeOffset?)null), cancellationToken);
        return affectedRows == 1;
    }

    public async Task<bool> MarkFailedAsync(Guid id, string error, string owner, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var normalizedError = error[..Math.Min(error.Length, 2000)];
        var affectedRows = await context.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE outbox_messages
            SET attempts = attempts + 1,
                last_error = {normalizedError},
                claimed_by = NULL,
                claimed_until = NULL,
                next_attempt_at = CASE
                    WHEN attempts + 1 >= 10 THEN NULL
                    ELSE {now} + (LEAST(300, POWER(2, LEAST(attempts + 1, 8))) * INTERVAL '1 second')
                END,
                dead_lettered_at = CASE
                    WHEN attempts + 1 >= 10 THEN {now}
                    ELSE NULL
                END
            WHERE id = {id}
              AND claimed_by = {owner}
              AND processed_at IS NULL
            """, cancellationToken);
        return affectedRows == 1;
    }

    public async Task PruneProcessedAsync(TimeSpan retention, int batchSize, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(retention);
        var ids = await context.OutboxMessages
            .Where(message => message.ProcessedAt != null && message.ProcessedAt < cutoff)
            .OrderBy(message => message.ProcessedAt)
            .Select(message => message.Id)
            .Take(batchSize)
            .ToArrayAsync(cancellationToken);
        await context.OutboxMessages
            .Where(message => ids.Contains(message.Id))
            .ExecuteDeleteAsync(cancellationToken);
    }
}

public static class OutboxServiceDependencyInjection
{
    public static IServiceCollection AddOutboxService(this IServiceCollection services)
    {
        services.AddScoped<IOutboxRepository, OutboxService>();
        return services;
    }
}
