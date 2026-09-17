using CleanArchitecture.Application.Features.Role.Assign;
using CleanArchitecture.Application.Features.Role.Create;
using CleanArchitecture.Application.Features.Role.Delete;
using CleanArchitecture.Application.Features.Role.GetAll;
using CleanArchitecture.Application.Features.Role.GetById;
using CleanArchitecture.Application.Features.Role.Remove;
using CleanArchitecture.Application.Features.Role.Update;
using CleanArchitecture.Application.Messaging;
using CleanArchitecture.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.Api.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.SuperAdmin}")]
public class RolesController(ISender sender) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleCommand command, CancellationToken cancellationToken)
    {
        return HandleResult(await sender.Send(command, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await sender.Send(new GetRoleByIdQuery(id), cancellationToken));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetAllRolesQuery query, CancellationToken cancellationToken)
    {
        return HandleResult(await sender.Send(query, cancellationToken));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        return HandleResult(await sender.Send(command, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        return HandleResult(await sender.Send(new DeleteRoleCommand(id), cancellationToken));
    }

    [HttpPost("assign")]
    public async Task<IActionResult> Assign([FromBody] AssignRoleCommand command, CancellationToken cancellationToken)
    {
        return HandleResult(await sender.Send(command, cancellationToken));
    }

    [HttpPost("remove")]
    public async Task<IActionResult> Remove([FromBody] RemoveRoleCommand command, CancellationToken cancellationToken)
    {
        return HandleResult(await sender.Send(command, cancellationToken));
    }
}
