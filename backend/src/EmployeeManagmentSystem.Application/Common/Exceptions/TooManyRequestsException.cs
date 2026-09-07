namespace EmployeeManagmentSystem.Application.Common.Exceptions;

public sealed class TooManyRequestsException : Exception
{
    public int RetryAfterSeconds { get; }

    public TooManyRequestsException()
        : this(60)
    {
    }

    public TooManyRequestsException(int retryAfterSeconds)
        : base("Too many login attempts. Try again later.")
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}
