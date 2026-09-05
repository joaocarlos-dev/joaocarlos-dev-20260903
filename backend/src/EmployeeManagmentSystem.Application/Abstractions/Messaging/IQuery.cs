using MediatR;

namespace EmployeeManagmentSystem.Application.Abstractions.Messaging;

public interface IQuery<TResult> : IRequest<TResult>
{
}
