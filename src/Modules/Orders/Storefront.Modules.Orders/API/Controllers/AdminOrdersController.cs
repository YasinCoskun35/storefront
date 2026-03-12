using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Orders.Core.Application.Commands;
using Storefront.Modules.Orders.Core.Application.Queries;
using Storefront.Modules.Orders.Core.Domain.Enums;
using System.Security.Claims;

namespace Storefront.Modules.Orders.API.Controllers;

[ApiController]
[Route("api/admin/orders")]
[Authorize(Roles = "Admin")]
public class AdminOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get order statistics for admin dashboard
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOrderStatsQuery(), cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, new { error = result.Error.Code });
    }

    /// <summary>
    /// Get all orders (admin view - no company filter)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] string? status,
        [FromQuery] string? partnerCompanyId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPartnerOrdersQuery(partnerCompanyId ?? "", status, pageNumber, pageSize, AdminMode: true);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Get order details
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderDetails(string id)
    {
        var query = new GetOrderDetailsQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Code == "Order.NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(string id, [FromBody] UpdateStatusRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");
        
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";

        var command = new UpdateOrderStatusCommand(
            id,
            request.NewStatus,
            userId,
            userName,
            request.Notes
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Order status updated successfully" })
            : result.Error.Code == "Order.NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Set order pricing (admin only)
    /// </summary>
    [HttpPut("{id}/pricing")]
    public async Task<IActionResult> SetOrderPricing(string id, [FromBody] SetPricingRequest request)
    {
        var itemPricing = request.Items.Select(i =>
            new OrderItemPricingDto(i.OrderItemId, i.UnitPrice, i.Discount)).ToList();

        var command = new SetOrderPricingCommand(
            id,
            itemPricing,
            request.ShippingCost,
            request.TaxAmount,
            request.Discount,
            request.Currency,
            request.Notes
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Order pricing updated successfully" })
            : result.Error.Type == "NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Add comment to order
    /// </summary>
    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(string id, [FromBody] AddAdminCommentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");
        
        var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";

        var command = new AddOrderCommentCommand(
            id,
            request.Content,
            request.Type,
            userId,
            userName,
            "Admin",
            request.IsInternal,
            request.AttachmentUrl,
            request.AttachmentFileName
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Created($"/api/admin/orders/{id}/comments/{result.Value}", new { commentId = result.Value })
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Update shipping info and tracking for an order
    /// </summary>
    [HttpPut("{id}/shipping")]
    public async Task<IActionResult> UpdateShipping(string id, [FromBody] UpdateShippingRequest request)
    {
        var command = new UpdateShippingInfoCommand(
            id, request.TrackingNumber, request.ShippingProvider,
            request.ExpectedDeliveryDate, request.ShippingNotes);

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Shipping info updated" })
            : result.Error.Type == "NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

public record UpdateStatusRequest(
    OrderStatus NewStatus,
    string? Notes
);

public record SetPricingRequest(
    List<ItemPricingDto> Items,
    decimal? ShippingCost,
    decimal? TaxAmount,
    decimal? Discount,
    string? Currency,
    string? Notes
);

public record ItemPricingDto(string OrderItemId, decimal UnitPrice, decimal? Discount);

public record AddAdminCommentRequest(
    string Content,
    CommentType Type,
    bool IsInternal,
    string? AttachmentUrl,
    string? AttachmentFileName
);

public record UpdateShippingRequest(
    string TrackingNumber,
    string ShippingProvider,
    DateTime? ExpectedDeliveryDate,
    string? ShippingNotes
);
