using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record UpdateShippingInfoCommand(
    string OrderId,
    string TrackingNumber,
    string ShippingProvider,
    DateTime? ExpectedDeliveryDate,
    string? ShippingNotes
) : IRequest<Result>;

public class UpdateShippingInfoCommandHandler : IRequestHandler<UpdateShippingInfoCommand, Result>
{
    private readonly OrdersDbContext _context;

    public UpdateShippingInfoCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateShippingInfoCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure(Error.NotFound("Order.NotFound", "Order not found."));

        order.TrackingNumber = request.TrackingNumber;
        order.ShippingProvider = request.ShippingProvider;
        order.ExpectedDeliveryDate = request.ExpectedDeliveryDate;

        if (order.Status == OrderStatus.ReadyToShip)
            order.Status = OrderStatus.Shipping;

        if (!string.IsNullOrEmpty(request.ShippingNotes))
        {
            var comment = new Domain.Entities.OrderComment
            {
                OrderId = order.Id,
                Content = $"Shipping updated. Carrier: {request.ShippingProvider}, Tracking: {request.TrackingNumber}. {request.ShippingNotes}",
                Type = Domain.Enums.CommentType.Shipping,
                AuthorId = "system",
                AuthorName = "Admin",
                AuthorType = "Admin",
                IsInternal = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.OrderComments.Add(comment);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
