using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record CancelOrderCommand(string OrderId, string PartnerUserId, string Reason) : IRequest<Result>;

public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
{
    private readonly OrdersDbContext _context;

    public CancelOrderCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.PartnerUserId == request.PartnerUserId, cancellationToken);

        if (order is null)
            return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

        var cancellableStatuses = new[] { OrderStatus.Pending, OrderStatus.QuoteSent };
        if (!cancellableStatuses.Contains(order.Status))
            return Result.Failure(Error.Validation("Order.CannotCancel", "Order can only be cancelled when pending or awaiting quote acceptance."));

        order.Status = OrderStatus.Cancelled;
        order.Notes = string.IsNullOrEmpty(order.Notes)
            ? $"Cancelled by partner: {request.Reason}"
            : $"{order.Notes}\n\nCancelled by partner: {request.Reason}";

        var comment = new Domain.Entities.OrderComment
        {
            OrderId = order.Id,
            Content = $"Order cancelled by partner. Reason: {request.Reason}",
            Type = Domain.Enums.CommentType.StatusChange,
            AuthorId = request.PartnerUserId,
            AuthorName = "Partner",
            AuthorType = "Partner",
            IsInternal = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.OrderComments.Add(comment);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
