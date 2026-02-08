using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Orders.Core.Application.Commands;
using Storefront.Modules.Orders.Core.Application.Queries;
using System.Security.Claims;

namespace Storefront.Modules.Orders.API.Controllers;

[ApiController]
[Route("api/partner/cart")]
[Authorize] // Partner authentication
public class CartsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CartsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get partner's shopping cart
    /// </summary>
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

    /// <summary>
    /// Add item to cart
    /// </summary>
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
            request.ColorChartId,
            request.ColorChartName,
            request.ColorOptionId,
            request.ColorOptionName,
            request.ColorOptionCode,
            request.CustomizationNotes
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { cartId = result.Value, message = "Item added to cart" })
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

public record AddToCartRequest(
    string ProductId,
    string ProductName,
    string ProductSKU,
    string? ProductImageUrl,
    int Quantity,
    string? ColorChartId,
    string? ColorChartName,
    string? ColorOptionId,
    string? ColorOptionName,
    string? ColorOptionCode,
    string? CustomizationNotes
);
