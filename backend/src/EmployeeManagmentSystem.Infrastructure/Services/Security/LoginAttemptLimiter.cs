using EmployeeManagmentSystem.Application.Abstractions.Security;
using EmployeeManagmentSystem.Application.Common.Configuration;
using EmployeeManagmentSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EmployeeManagmentSystem.Infrastructure.Services.Security;

public sealed class LoginAttemptLimiter(
    EmployeeManagementDbContext context,
    LoginRateLimitOptions options) : ILoginAttemptLimiter
{
    public int RetryAfterSeconds => options.WindowSeconds;

    public async Task<bool> IsAllowedAsync(
        string login,
        string? clientIp,
        CancellationToken cancellationToken = default)
    {
        var keys = new[]
        {
            $"account:{login.Trim().ToUpperInvariant()}",
            $"ip:{clientIp ?? "unknown"}"
        };

        await using var transaction = await context.Database.BeginTransactionAsync(
            System.Data.IsolationLevel.ReadCommitted,
            cancellationToken);
        try
        {
            await ExecuteAsync(
                transaction,
                """
                WITH expired AS (
                    SELECT key
                    FROM login_rate_limit_buckets
                    WHERE window_started < NOW() - (@retention_seconds * INTERVAL '1 second')
                    ORDER BY window_started
                    LIMIT 100
                )
                DELETE FROM login_rate_limit_buckets
                WHERE key IN (SELECT key FROM expired);
                """,
                null,
                options.WindowSeconds * 2,
                cancellationToken);

            var allowed = true;
            foreach (var key in keys)
            {
                var count = await UpsertAndReadCountAsync(transaction, key, cancellationToken);
                allowed &= count <= options.PermitLimit;
            }

            await transaction.CommitAsync(cancellationToken);
            return allowed;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task<int> UpsertAndReadCountAsync(
        IDbContextTransaction transaction,
        string key,
        CancellationToken cancellationToken)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = """
            INSERT INTO login_rate_limit_buckets (key, window_started, attempt_count)
            VALUES (@key, NOW(), 1)
            ON CONFLICT (key) DO UPDATE
            SET window_started = CASE
                    WHEN NOW() - login_rate_limit_buckets.window_started >= (@window_seconds * INTERVAL '1 second')
                    THEN NOW()
                    ELSE login_rate_limit_buckets.window_started
                END,
                attempt_count = CASE
                    WHEN NOW() - login_rate_limit_buckets.window_started >= (@window_seconds * INTERVAL '1 second')
                    THEN 1
                    ELSE login_rate_limit_buckets.attempt_count + 1
                END;
            SELECT attempt_count FROM login_rate_limit_buckets WHERE key = @key;
            """;
        AddParameter(command, "key", key);
        AddParameter(command, "window_seconds", options.WindowSeconds);
        return Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));
    }

    private static async Task ExecuteAsync(
        IDbContextTransaction transaction,
        string commandText,
        string? key,
        int retentionSeconds,
        CancellationToken cancellationToken)
    {
        await using var command = transaction.GetDbTransaction().Connection!.CreateCommand();
        command.Transaction = transaction.GetDbTransaction();
        command.CommandText = commandText;
        AddParameter(command, "retention_seconds", retentionSeconds);
        if (key is not null)
        {
            AddParameter(command, "key", key);
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddParameter(System.Data.Common.DbCommand command, string name, object value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value;
        command.Parameters.Add(parameter);
    }
}
