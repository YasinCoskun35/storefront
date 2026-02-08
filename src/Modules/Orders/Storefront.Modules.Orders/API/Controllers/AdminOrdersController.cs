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
    /// Get all orders (admin view)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] string? status,
        [FromQuery] string? partnerCompanyId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        // TODO: Create GetAllOrdersQuery that doesn't filter by companyId
        var query = new GetPartnerOrdersQuery(partnerCompanyId ?? "", status, pageNumber, pageSize);
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
}

public record UpdateStatusRequest(
    OrderStatus NewStatus,
    string? Notes
);

public record AddAdminCommentRequest(
    string Content,
    CommentType Type,
    bool IsInternal,
    string? AttachmentUrl,
    string? AttachmentFileName
);
