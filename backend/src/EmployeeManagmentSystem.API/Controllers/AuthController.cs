using EmployeeManagmentSystem.Application.DTOs;
using EmployeeManagmentSystem.Application.Queries.Authentication.Authenticate;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagmentSystem.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthenticationDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthenticationDto>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AuthenticateQuery(request.Login, request.Password), cancellationToken);
        return Ok(result);
    }
}

public sealed record LoginRequest(string Login, string Password);
