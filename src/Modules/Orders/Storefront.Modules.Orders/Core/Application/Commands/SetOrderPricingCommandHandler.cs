using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class SetOrderPricingCommandHandler : IRequestHandler<SetOrderPricingCommand, Result>
{
    private readonly OrdersDbContext _context;

    public SetOrderPricingCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SetOrderPricingCommand request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
            return Result.Failure(Error.NotFound("Order.NotFound", $"Order '{request.OrderId}' not found."));

        // Update item-level pricing
        foreach (var itemPricing in request.ItemPricing)
        {
            var item = order.Items.FirstOrDefault(i => i.Id == itemPricing.OrderItemId);
            if (item is null) continue;

            item.UnitPrice = itemPricing.UnitPrice;
            item.Discount = itemPricing.Discount ?? 0;
            item.TotalPrice = (itemPricing.UnitPrice * item.Quantity) - (itemPricing.Discount ?? 0);
        }

        // Update order-level totals
        order.ShippingCost = request.ShippingCost;
        order.TaxAmount = request.TaxAmount;
        order.Discount = request.Discount;
        order.Currency = request.Currency ?? "USD";

        // Calculate subtotal from items
        order.SubTotal = order.Items.Sum(i => i.TotalPrice ?? 0);

        // Total = subtotal + shipping + tax - discount
        order.TotalAmount = (order.SubTotal ?? 0)
            + (order.ShippingCost ?? 0)
            + (order.TaxAmount ?? 0)
            - (order.Discount ?? 0);

        if (!string.IsNullOrEmpty(request.Notes))
            order.Notes = request.Notes;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
