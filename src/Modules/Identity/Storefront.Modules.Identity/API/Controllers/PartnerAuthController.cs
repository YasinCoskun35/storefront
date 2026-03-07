using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Identity.Core.Application.Commands;
using Storefront.Modules.Identity.Core.Application.Queries;
using System.Security.Claims;

namespace Storefront.Modules.Identity.API.Controllers;

[ApiController]
[Route("api/identity/partners")]
public class PartnerAuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public PartnerAuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Partner login
    /// </summary>
    [HttpPost("auth/login")]
    public async Task<IActionResult> Login([FromBody] PartnerLoginCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Get current partner profile
    /// </summary>
    [HttpGet("profile")]
    [Authorize(Roles = "Partner")]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var result = await _mediator.Send(new GetPartnerProfileQuery(userId), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error.Code, message = result.Error.Message });
    }
}
