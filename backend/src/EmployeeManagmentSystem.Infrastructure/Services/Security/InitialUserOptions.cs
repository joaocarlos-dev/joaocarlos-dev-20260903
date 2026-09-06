namespace EmployeeManagmentSystem.Infrastructure.Services.Security;

public sealed class InitialUserOptions
{
    public string Code { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public string? Password { get; init; }
    public bool IsConfigured => !string.IsNullOrWhiteSpace(Password);

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Code))
        {
            throw new InvalidOperationException("InitialUser:Code is required when an initial password is configured.");
        }

        if (string.IsNullOrWhiteSpace(Login))
        {
            throw new InvalidOperationException("InitialUser:Login is required when an initial password is configured.");
        }

        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
        {
            throw new InvalidOperationException("InitialUser:Password must contain at least 8 characters.");
        }
    }
}
