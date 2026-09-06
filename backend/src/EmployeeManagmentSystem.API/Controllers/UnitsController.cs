using EmployeeManagmentSystem.Application.Commands.Units.CreateUnit;
using EmployeeManagmentSystem.Application.Commands.Units.UpdateUnit;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Application.Queries.Units.GetUnit;
using EmployeeManagmentSystem.Application.Queries.Units.GetUnits;
using EmployeeManagmentSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/units")]
public sealed class UnitsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(
        CreateUnitRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(new CreateUnitCommand(request.Code, request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UnitDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UnitDto>>> GetAll(CancellationToken cancellationToken)
    {
        var units = await sender.Send(new GetUnitsQuery(), cancellationToken);
        return Ok(units);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UnitDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UnitDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var unit = await sender.Send(new GetUnitQuery(id), cancellationToken);
        return Ok(unit);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateUnitRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateUnitCommand(id, request.Name, request.Status), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateUnitRequest(string Code, string Name);

public sealed record UpdateUnitRequest(string? Name, EntityStatus? Status);
