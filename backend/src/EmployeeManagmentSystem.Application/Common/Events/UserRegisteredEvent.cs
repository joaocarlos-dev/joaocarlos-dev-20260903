namespace EmployeeManagmentSystem.Application.Common.Events;

public sealed record UserRegisteredEvent(Guid UserId, string Code, string Login, DateTimeOffset OccurredAt);
