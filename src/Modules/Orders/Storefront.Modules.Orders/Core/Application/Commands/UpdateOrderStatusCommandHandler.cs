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

    public UpdateOrderStatusCommandHandler(OrdersDbContext context)
    {
        _context = context;
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

        return Result.Success();
    }
}
