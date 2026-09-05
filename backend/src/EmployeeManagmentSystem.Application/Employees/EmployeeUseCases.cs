using EmployeeManagmentSystem.Application.Abstractions.Messaging;
using EmployeeManagmentSystem.Application.Abstractions.Persistence;
using EmployeeManagmentSystem.Application.Common.Exceptions;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Domain.Entities;
using FluentValidation;
using MediatR;

namespace EmployeeManagmentSystem.Application.Employees;

public sealed record CreateEmployeeCommand(string Code, string Name, Guid UserId, Guid UnitId) : ICommand<Guid>;

public sealed record UpdateEmployeeCommand(Guid Id, string? Name, Guid? UnitId) : ICommand;

public sealed record DeleteEmployeeCommand(Guid Id) : ICommand;

public sealed record GetEmployeeQuery(Guid Id) : IQuery<EmployeeDto>;

public sealed record ListEmployeesQuery : IQuery<IReadOnlyCollection<EmployeeDto>>;

internal sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(command => command.Code).NotEmpty();
        RuleFor(command => command.Name).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.UnitId).NotEmpty();
    }
}

internal sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Name).NotEmpty().When(command => command.Name is not null);
        RuleFor(command => command.UnitId).NotEmpty().When(command => command.UnitId.HasValue);
        RuleFor(command => command).Must(command => command.Name is not null || command.UnitId.HasValue)
            .WithMessage("At least one employee field must be provided for update.");
    }
}

internal sealed class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}

internal sealed class GetEmployeeQueryValidator : AbstractValidator<GetEmployeeQuery>
{
    public GetEmployeeQueryValidator()
    {
        RuleFor(query => query.Id).NotEmpty();
    }
}

internal sealed class CreateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IUnitRepository unitRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateEmployeeCommand, Guid>
{
    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var code = request.Code.Trim();

        if (await employeeRepository.CodeExistsAsync(code, cancellationToken))
        {
            throw new ApplicationConflictException("The employee code is already in use.");
        }

        if (await employeeRepository.UserIsLinkedAsync(request.UserId, cancellationToken))
        {
            throw new ApplicationConflictException("The user is already linked to an employee.");
        }

        _ = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.UserId);
        var unit = await unitRepository.GetByIdAsync(request.UnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(EmployeeManagmentSystem.Domain.Entities.Unit), request.UnitId);

        var employee = new Employee(code, request.Name, request.UserId, unit);
        await employeeRepository.AddAsync(employee, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return employee.Id;
    }
}

internal sealed class UpdateEmployeeCommandHandler(
    IEmployeeRepository employeeRepository,
    IUnitRepository unitRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateEmployeeCommand>
{
    public async Task Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.Id);

        if (request.Name is not null)
        {
            employee.UpdateName(request.Name);
        }

        if (request.UnitId.HasValue)
        {
            var unit = await unitRepository.GetByIdAsync(request.UnitId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(EmployeeManagmentSystem.Domain.Entities.Unit), request.UnitId.Value);
            employee.TransferTo(unit);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class DeleteEmployeeCommandHandler(IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteEmployeeCommand>
{
    public async Task Handle(DeleteEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.Id);

        employee.Delete();
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal sealed class GetEmployeeQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<GetEmployeeQuery, EmployeeDto>
{
    public async Task<EmployeeDto> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
    {
        var employee = await employeeRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Employee), request.Id);

        return EmployeeDto.FromEntity(employee);
    }
}

internal sealed class ListEmployeesQueryHandler(IEmployeeRepository employeeRepository)
    : IRequestHandler<ListEmployeesQuery, IReadOnlyCollection<EmployeeDto>>
{
    public async Task<IReadOnlyCollection<EmployeeDto>> Handle(
        ListEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var employees = await employeeRepository.ListAsync(cancellationToken);
        return employees.Select(EmployeeDto.FromEntity).ToArray();
    }
}
