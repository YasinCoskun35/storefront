using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Orders.Core.Application.Commands;
using Storefront.Modules.Orders.Core.Application.Queries;
using System.Security.Claims;

namespace Storefront.Modules.Orders.API.Controllers;

[ApiController]
[Route("api/partner/saved-addresses")]
[Authorize]
public class PartnerSavedAddressesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PartnerSavedAddressesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSavedAddresses(CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _mediator.Send(new GetSavedAddressesQuery(userId), ct);
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSavedAddress([FromBody] CreateSavedAddressRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var companyId = User.FindFirst("companyId")?.Value!;

        var command = new CreateSavedAddressCommand(
            userId, companyId,
            request.Label, request.Address, request.City,
            request.State, request.PostalCode, request.Country,
            request.IsDefault
        );

        var result = await _mediator.Send(command, ct);
        return result.IsSuccess
            ? Created($"/api/partner/saved-addresses/{result.Value}", new { id = result.Value })
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSavedAddress(string id, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
        var result = await _mediator.Send(new DeleteSavedAddressCommand(id, userId), ct);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == "NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

public record CreateSavedAddressRequest(
    string Label,
    string Address,
    string City,
    string State,
    string PostalCode,
    string Country,
    bool IsDefault
);
