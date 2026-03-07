using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Orders.Core.Application.Commands;
using Storefront.Modules.Orders.Core.Application.Queries;
using System.Security.Claims;

namespace Storefront.Modules.Orders.API.Controllers;

[ApiController]
[Route("api/partner/cart")]
[Authorize]
public class CartsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var query = new GetCartQuery(userId);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    [HttpDelete("items/{itemId}")]
    public async Task<IActionResult> RemoveFromCart(string itemId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var command = new RemoveCartItemCommand(userId, itemId);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? NoContent()
            : result.Error.Type == "NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    [HttpPatch("items/{itemId}")]
    public async Task<IActionResult> UpdateCartItemQuantity(string itemId, [FromBody] UpdateQuantityRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var command = new UpdateCartItemQuantityCommand(userId, itemId, request.Quantity);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Quantity updated" })
            : result.Error.Type == "NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var companyId = User.FindFirst("companyId")?.Value
            ?? throw new UnauthorizedAccessException("Company ID not found");

        var command = new AddToCartCommand(
            userId,
            companyId,
            request.ProductId,
            request.ProductName,
            request.ProductSKU,
            request.ProductImageUrl,
            request.Quantity,
            request.SelectedVariants,
            request.CustomizationNotes
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { cartId = result.Value, message = "Item added to cart" })
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

public record UpdateQuantityRequest(int Quantity);

public record AddToCartRequest(
    string ProductId,
    string ProductName,
    string ProductSKU,
    string? ProductImageUrl,
    int Quantity,
    string? SelectedVariants,
    string? CustomizationNotes
);
