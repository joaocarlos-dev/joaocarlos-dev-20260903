namespace EmployeeManagmentSystem.Infrastructure.Services.Outbox;

public sealed class RabbitMqOptions
{
    public bool Enabled { get; init; }
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string UserName { get; init; } = "guest";
    public string Password { get; init; } = "guest";
    public string Exchange { get; init; } = "employee-management.events.v2";
    public string Queue { get; init; } = "employee-management.user-registrations.v2";
    public string UnroutedExchange { get; init; } = "employee-management.unrouted.v2";
    public string UnroutedQueue { get; init; } = "employee-management.unrouted.v2";
    public int UnroutedMessageTtlHours { get; init; } = 168;
    public int UnroutedQueueMaxLength { get; init; } = 10000;
    public int PollingSeconds { get; init; } = 5;
    public int ProcessedRetentionDays { get; init; } = 30;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Host) || Port is <= 0 or > 65535 || string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            throw new InvalidOperationException("RabbitMq connection settings are invalid.");
        }

        if (string.IsNullOrWhiteSpace(Exchange) || string.IsNullOrWhiteSpace(Queue) || string.IsNullOrWhiteSpace(UnroutedExchange) || string.IsNullOrWhiteSpace(UnroutedQueue) || PollingSeconds <= 0 || ProcessedRetentionDays <= 0 || UnroutedMessageTtlHours <= 0 || UnroutedQueueMaxLength <= 0)
        {
            throw new InvalidOperationException("RabbitMq topology and timing settings are invalid.");
        }
    }
}
