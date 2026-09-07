namespace EmployeeManagmentSystem.Application.Abstractions.Security;

public interface ILoginAttemptLimiter
{
    int RetryAfterSeconds { get; }
    Task<bool> IsAllowedAsync(string login, string? clientIp, CancellationToken cancellationToken = default);
}
