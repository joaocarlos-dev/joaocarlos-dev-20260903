namespace EmployeeManagmentSystem.Application.Common.Events;

public sealed class OutboxMessage
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Type { get; init; }
    public required string Payload { get; init; }
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public DateTimeOffset? ClaimedUntil { get; set; }
    public string? ClaimedBy { get; set; }
    public DateTimeOffset? DeadLetteredAt { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
