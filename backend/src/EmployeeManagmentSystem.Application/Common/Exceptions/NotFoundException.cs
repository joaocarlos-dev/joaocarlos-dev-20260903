namespace EmployeeManagmentSystem.Application.Common.Exceptions;

public sealed class NotFoundException(string resourceName, Guid id)
    : Exception($"{resourceName} with identifier '{id}' was not found.");
