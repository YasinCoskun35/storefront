using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Entities;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class UpdateOrderStatusCommandHandler : IRequestHandler<UpdateOrderStatusCommand, Result>
{
    private readonly OrdersDbContext _context;
    private readonly IPartnerAccountService _partnerAccountService;
    private readonly IExpoPushService _pushService;
    private readonly IPartnerPushTokenResolver _pushTokenResolver;

    public UpdateOrderStatusCommandHandler(
        OrdersDbContext context,
        IPartnerAccountService partnerAccountService,
        IExpoPushService pushService,
        IPartnerPushTokenResolver pushTokenResolver)
    {
        _context            = context;
        _partnerAccountService = partnerAccountService;
        _pushService        = pushService;
        _pushTokenResolver  = pushTokenResolver;
    }

    public async Task<Result> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Comments)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Order not found");
        }

        var oldStatus = order.Status;
        order.Status = request.NewStatus;
        order.UpdatedAt = DateTime.UtcNow;

        // Update specific timestamps based on status
        switch (request.NewStatus)
        {
            case OrderStatus.Confirmed:
                order.ConfirmedAt = DateTime.UtcNow;
                break;
            case OrderStatus.Cancelled:
                order.CancelledAt = DateTime.UtcNow;
                break;
            case OrderStatus.Delivered:
                order.ActualDeliveryDate = DateTime.UtcNow;
                break;
        }

        // Add system comment about status change
        var statusComment = new OrderComment
        {
            OrderId = order.Id,
            Content = $"Order status changed from {oldStatus} to {request.NewStatus}" +
                      (string.IsNullOrEmpty(request.Notes) ? "" : $"\n\nNotes: {request.Notes}"),
            Type = CommentType.StatusChange,
            AuthorId = request.UpdatedBy,
            AuthorName = request.UpdatedByName,
            AuthorType = "Admin",
            IsSystemGenerated = true,
            CreatedAt = DateTime.UtcNow
        };

        order.Comments.Add(statusComment);
        await _context.SaveChangesAsync(cancellationToken);

        // When confirmed with a priced total, debit the partner's current account
        if (request.NewStatus == OrderStatus.Confirmed
            && !string.IsNullOrEmpty(order.PartnerCompanyId)
            && order.TotalAmount.HasValue
            && order.TotalAmount.Value > 0)
        {
            await _partnerAccountService.RecordOrderDebitAsync(
                order.PartnerCompanyId,
                order.OrderNumber,
                order.TotalAmount.Value,
                request.UpdatedBy,
                cancellationToken);
        }

        // Send push notification to partner (fire-and-forget, never blocks the response)
        if (!string.IsNullOrEmpty(order.PartnerUserId))
        {
            _ = SendPushNotificationAsync(order, request.NewStatus, cancellationToken);
        }

        return Result.Success();
    }

    private async Task SendPushNotificationAsync(Order order, OrderStatus newStatus, CancellationToken ct)
    {
        try
        {
            var token = await _pushTokenResolver.GetPushTokenAsync(order.PartnerUserId!, ct);
            if (string.IsNullOrEmpty(token))
                return;

            var (title, body) = BuildNotificationText(order.OrderNumber, newStatus);
            await _pushService.SendAsync(token, title, body,
                new { orderId = order.Id, orderNumber = order.OrderNumber, status = newStatus.ToString() },
                ct);
        }
        catch
        {
            // Never propagate push failures — order update already succeeded
        }
    }

    private static (string Title, string Body) BuildNotificationText(string orderNumber, OrderStatus status)
        => status switch
        {
            OrderStatus.Confirmed      => ("Order Confirmed", $"Your order {orderNumber} has been confirmed."),
            OrderStatus.Preparing      => ("Order Preparing", $"Your order {orderNumber} is being prepared."),
            OrderStatus.ReadyToShip    => ("Ready to Ship", $"Your order {orderNumber} is ready to ship."),
            OrderStatus.Shipping       => ("Order Shipped", $"Your order {orderNumber} is on its way!"),
            OrderStatus.Delivered      => ("Order Delivered", $"Your order {orderNumber} has been delivered."),
            OrderStatus.Cancelled      => ("Order Cancelled", $"Your order {orderNumber} has been cancelled."),
            OrderStatus.PendingPayment => ("Payment Required", $"Order {orderNumber} is awaiting payment."),
            _                          => ("Order Update", $"Your order {orderNumber} status has been updated to {status}.")
        };
}
