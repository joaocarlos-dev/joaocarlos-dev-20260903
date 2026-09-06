using EmployeeManagmentSystem.Application.Commands.Employees.CreateEmployee;
using EmployeeManagmentSystem.Application.Commands.Employees.DeleteEmployee;
using EmployeeManagmentSystem.Application.Commands.Employees.UpdateEmployee;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Application.Queries.Employees.GetEmployee;
using EmployeeManagmentSystem.Application.Queries.Employees.GetEmployees;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/employees")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public sealed class EmployeesController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Create(
        CreateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            new CreateEmployeeCommand(request.Code, request.Name, request.UserId, request.UnitId),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<EmployeeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var employees = await sender.Send(new GetEmployeesQuery(), cancellationToken);
        return Ok(employees);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var employee = await sender.Send(new GetEmployeeQuery(id), cancellationToken);
        return Ok(employee);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateEmployeeCommand(id, request.Name, request.UnitId), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteEmployeeCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateEmployeeRequest(string Code, string Name, Guid UserId, Guid UnitId);

public sealed record UpdateEmployeeRequest(string? Name, Guid? UnitId);
