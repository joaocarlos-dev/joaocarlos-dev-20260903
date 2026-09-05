using MediatR;

namespace EmployeeManagmentSystem.Application.Abstractions.Messaging;

public interface ICommand : IRequest
{
}

public interface ICommand<TResult> : IRequest<TResult>
{
}
