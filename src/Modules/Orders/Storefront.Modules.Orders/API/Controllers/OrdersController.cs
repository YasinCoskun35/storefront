using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Orders.Core.Application.Commands;
using Storefront.Modules.Orders.Core.Application.Queries;
using Storefront.Modules.Orders.Core.Domain.Enums;
using System.Security.Claims;

namespace Storefront.Modules.Orders.API.Controllers;

[ApiController]
[Route("api/partner/orders")]
[Authorize] // Partner authentication
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get partner order statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var companyId = User.FindFirst("companyId")?.Value ?? "";
        var result = await _mediator.Send(new GetPartnerOrderStatsQuery(companyId));
        return result.IsSuccess ? Ok(result.Value) : StatusCode(500, new { error = result.Error.Code });
    }

    /// <summary>
    /// Get partner's orders
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetOrders(
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var companyId = User.FindFirst("companyId")?.Value
            ?? throw new UnauthorizedAccessException("Company ID not found");

        var query = new GetPartnerOrdersQuery(companyId, status, pageNumber, pageSize);
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
        var query = new GetOrderDetailsQuery(id, IncludeInternalComments: false);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Code == "Order.NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Create order from cart
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var companyId = User.FindFirst("companyId")?.Value
            ?? throw new UnauthorizedAccessException("Company ID not found");

        var companyName = User.FindFirst("companyName")?.Value ?? "Partner Company";

        var command = new CreateOrderCommand(
            userId,
            companyId,
            companyName,
            request.DeliveryAddress,
            request.DeliveryCity,
            request.DeliveryState,
            request.DeliveryPostalCode,
            request.DeliveryCountry,
            request.DeliveryNotes,
            request.RequestedDeliveryDate,
            request.Notes
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Created($"/api/partner/orders/{result.Value}", new { orderId = result.Value })
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Add comment to order
    /// </summary>
    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(string id, [FromBody] AddCommentRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");
        
        var userName = $"{User.FindFirst(ClaimTypes.GivenName)?.Value} {User.FindFirst(ClaimTypes.Surname)?.Value}";

        var command = new AddOrderCommentCommand(
            id,
            request.Content,
            request.Type,
            userId,
            userName,
            "Partner",
            false, // Partner comments are not internal
            request.AttachmentUrl,
            request.AttachmentFileName
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Created($"/api/partner/orders/{id}/comments/{result.Value}", new { commentId = result.Value })
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Cancel an order (only while pending/quote sent)
    /// </summary>
    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(string id, [FromBody] CancelOrderRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var result = await _mediator.Send(new CancelOrderCommand(id, userId, request.Reason));

        return result.IsSuccess
            ? Ok(new { message = "Order cancelled" })
            : result.Error.Type == "NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }
}

public record CreateOrderRequest(
    string DeliveryAddress,
    string DeliveryCity,
    string DeliveryState,
    string DeliveryPostalCode,
    string DeliveryCountry,
    string? DeliveryNotes,
    DateTime? RequestedDeliveryDate,
    string? Notes
);

public record AddCommentRequest(
    string Content,
    CommentType Type,
    string? AttachmentUrl,
    string? AttachmentFileName
);

public record CancelOrderRequest(string Reason);
