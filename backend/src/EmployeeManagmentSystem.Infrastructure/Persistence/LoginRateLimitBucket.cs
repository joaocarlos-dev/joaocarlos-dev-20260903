namespace EmployeeManagmentSystem.Infrastructure.Persistence;

public sealed class LoginRateLimitBucket
{
    public required string Key { get; init; }
    public DateTimeOffset WindowStarted { get; set; }
    public int AttemptCount { get; set; }
}
