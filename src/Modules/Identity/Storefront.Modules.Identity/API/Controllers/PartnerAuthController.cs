using MediatR;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Identity.Core.Application.Commands;

namespace Storefront.Modules.Identity.API.Controllers;

[ApiController]
[Route("api/partners/auth")]
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
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] PartnerLoginCommand command)
    {
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}
