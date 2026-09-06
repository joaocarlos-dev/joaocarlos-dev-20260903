using EmployeeManagmentSystem.Application.Commands.Users.CreateUser;
using EmployeeManagmentSystem.Application.Commands.Users.UpdateUser;
using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Application.Queries.Users.GetUser;
using EmployeeManagmentSystem.Application.Queries.Users.GetUsers;
using EmployeeManagmentSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystem.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/users")]
public sealed class UsersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(
            new CreateUserCommand(request.Code, request.Login, request.Password, request.Status),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<UserDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetAll(
        [FromQuery] EntityStatus? status,
        CancellationToken cancellationToken)
    {
        var users = await sender.Send(new GetUsersQuery(status), cancellationToken);
        return Ok(users);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserQuery(id), cancellationToken);
        return Ok(user);
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateUserCommand(id, request.Password, request.Status), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateUserRequest(string Code, string Login, string Password, EntityStatus Status);

public sealed record UpdateUserRequest(string? Password, EntityStatus? Status);
