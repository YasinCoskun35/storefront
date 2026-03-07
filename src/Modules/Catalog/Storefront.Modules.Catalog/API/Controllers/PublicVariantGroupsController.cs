using MediatR;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Catalog.Core.Application.Queries;

namespace Storefront.Modules.Catalog.API.Controllers;

[ApiController]
[Route("api/products/{productId}/variant-groups")]
public sealed class PublicVariantGroupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PublicVariantGroupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProductVariantGroups(string productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductVariantGroupsQuery(productId), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, new { error = result.Error.Code, message = result.Error.Message });
    }
}
