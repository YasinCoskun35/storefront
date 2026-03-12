using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Identity.Core.Application.Queries;

namespace Storefront.Modules.Identity.API.Controllers;

[ApiController]
[Route("api/identity/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminUsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] string? search, CancellationToken cancellationToken)
    {
        var query = new GetAdminUsersQuery(search);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : StatusCode(500, new { error = result.Error.Code, message = result.Error.Message });
    }
}
