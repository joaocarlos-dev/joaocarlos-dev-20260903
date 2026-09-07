namespace EmployeeManagmentSystem.Application.Common.Events;

public sealed record UserRegisteredEvent(Guid EventId, Guid UserId, string Code, string Login, DateTimeOffset OccurredAt);
