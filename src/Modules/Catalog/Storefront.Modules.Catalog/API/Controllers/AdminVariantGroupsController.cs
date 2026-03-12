using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Catalog.Core.Application.Commands;
using Storefront.Modules.Catalog.Core.Application.Queries;

namespace Storefront.Modules.Catalog.API.Controllers;

[ApiController]
[Route("api/admin/variant-groups")]
[Authorize(Roles = "Admin")]
public sealed class AdminVariantGroupsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminVariantGroupsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? isActive, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVariantGroupsQuery(isActive), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, new { error = result.Error.Code, message = result.Error.Message });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetVariantGroupDetailsQuery(id), ct);
        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVariantGroupRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateVariantGroupCommand(
            req.Name, req.Description ?? string.Empty, req.DisplayType,
            req.IsRequired, req.AllowMultiple, req.DisplayOrder), ct);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
            : result.Error.Type switch
            {
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateVariantGroupRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateVariantGroupCommand(
            id, req.Name, req.Description ?? string.Empty, req.DisplayType,
            req.IsRequired, req.AllowMultiple, req.DisplayOrder, req.IsActive), ct);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteVariantGroupCommand(id), ct);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Conflict" => Conflict(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpPost("{id}/options")]
    public async Task<IActionResult> AddOption(string id, [FromBody] AddVariantOptionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddVariantOptionCommand(
            id, req.Name, req.Code, req.HexColor, req.ImageUrl,
            req.PriceAdjustment, req.IsAvailable, req.DisplayOrder), ct);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Conflict" => Conflict(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpPut("{id}/options/{optionId}")]
    public async Task<IActionResult> UpdateOption(string id, string optionId, [FromBody] UpdateVariantOptionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UpdateVariantOptionCommand(
            optionId, req.Name, req.Code, req.HexColor, req.ImageUrl,
            req.PriceAdjustment, req.IsAvailable, req.DisplayOrder), ct);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Conflict" => Conflict(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpDelete("{id}/options/{optionId}")]
    public async Task<IActionResult> DeleteOption(string id, string optionId, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteVariantOptionCommand(optionId), ct);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetProductVariantGroups(string productId, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductVariantGroupsQuery(productId), ct);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, new { error = result.Error.Code, message = result.Error.Message });
    }

    [HttpPost("product/{productId}")]
    public async Task<IActionResult> AssignToProduct(string productId, [FromBody] AssignVariantGroupRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new AssignVariantGroupToProductCommand(
            productId, req.VariantGroupId, req.IsRequired, req.DisplayOrder), ct);

        return result.IsSuccess
            ? Ok(new { id = result.Value })
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Conflict" => Conflict(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    [HttpDelete("product/{productId}/{groupId}")]
    public async Task<IActionResult> RemoveFromProduct(string productId, string groupId, CancellationToken ct)
    {
        var result = await _mediator.Send(new RemoveVariantGroupFromProductCommand(productId, groupId), ct);
        return result.IsSuccess
            ? NoContent()
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                _ => StatusCode(500, new { error = result.Error.Code, message = result.Error.Message })
            };
    }
}

public sealed record CreateVariantGroupRequest(
    string Name,
    string? Description,
    string DisplayType,
    bool IsRequired = true,
    bool AllowMultiple = false,
    int DisplayOrder = 0
);

public sealed record UpdateVariantGroupRequest(
    string Name,
    string? Description,
    string DisplayType,
    bool IsRequired,
    bool AllowMultiple,
    int DisplayOrder,
    bool IsActive
);

public sealed record AddVariantOptionRequest(
    string Name,
    string Code,
    string? HexColor,
    string? ImageUrl,
    decimal? PriceAdjustment,
    bool IsAvailable = true,
    int DisplayOrder = 0
);

public sealed record UpdateVariantOptionRequest(
    string Name,
    string Code,
    string? HexColor,
    string? ImageUrl,
    decimal? PriceAdjustment,
    bool IsAvailable,
    int DisplayOrder
);

public sealed record AssignVariantGroupRequest(
    string VariantGroupId,
    bool IsRequired = true,
    int DisplayOrder = 0
);
