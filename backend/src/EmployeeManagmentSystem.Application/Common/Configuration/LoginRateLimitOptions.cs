namespace EmployeeManagmentSystem.Application.Common.Configuration;

public sealed class LoginRateLimitOptions
{
    public int PermitLimit { get; init; } = 10;
    public int WindowSeconds { get; init; } = 60;
}
