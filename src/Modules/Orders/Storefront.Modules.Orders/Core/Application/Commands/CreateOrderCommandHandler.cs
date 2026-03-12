using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Entities;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<string>>
{
    private readonly OrdersDbContext _context;
    private readonly IPartnerDiscountResolver _discountResolver;

    public CreateOrderCommandHandler(OrdersDbContext context, IPartnerDiscountResolver discountResolver)
    {
        _context = context;
        _discountResolver = discountResolver;
    }

    public async Task<Result<string>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.PartnerUserId == request.PartnerUserId && c.IsActive, cancellationToken);

        if (cart is null || !cart.Items.Any())
        {
            return Error.Validation("Order.EmptyCart", "Cannot create order from empty cart");
        }

        var discountRate = await _discountResolver.GetDiscountRateAsync(request.PartnerCompanyId, cancellationToken);

        var orderCount = await _context.Orders.CountAsync(cancellationToken);
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyy}-{(orderCount + 1):D4}";

        var order = new Order
        {
            OrderNumber = orderNumber,
            PartnerCompanyId = request.PartnerCompanyId,
            PartnerUserId = request.PartnerUserId,
            PartnerCompanyName = request.PartnerCompanyName,
            Status = OrderStatus.Pending,
            DeliveryAddress = request.DeliveryAddress,
            DeliveryCity = request.DeliveryCity,
            DeliveryState = request.DeliveryState,
            DeliveryPostalCode = request.DeliveryPostalCode,
            DeliveryCountry = request.DeliveryCountry,
            DeliveryNotes = request.DeliveryNotes,
            RequestedDeliveryDate = request.RequestedDeliveryDate.HasValue
                ? DateTime.SpecifyKind(request.RequestedDeliveryDate.Value, DateTimeKind.Utc)
                : null,
            Notes = request.Notes,
            Currency = "TRY",
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow
        };

        decimal subTotal = 0;
        decimal totalDiscount = 0;

        foreach (var cartItem in cart.Items)
        {
            decimal? discountedPrice = null;
            decimal? itemDiscount = null;
            decimal? totalPrice = null;

            if (cartItem.UnitPrice.HasValue && cartItem.UnitPrice.Value > 0)
            {
                var catalogPrice = cartItem.UnitPrice.Value;
                discountedPrice = discountRate > 0
                    ? Math.Round(catalogPrice * (1 - discountRate / 100m), 2)
                    : catalogPrice;
                itemDiscount = Math.Round((catalogPrice - discountedPrice.Value) * cartItem.Quantity, 2);
                totalPrice = Math.Round(discountedPrice.Value * cartItem.Quantity, 2);

                subTotal += discountedPrice.Value * cartItem.Quantity;
                totalDiscount += itemDiscount.Value;
            }

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.ProductName,
                ProductSKU = cartItem.ProductSKU,
                ProductImageUrl = cartItem.ProductImageUrl,
                Quantity = cartItem.Quantity,
                SelectedVariants = cartItem.SelectedVariants,
                CustomizationNotes = cartItem.CustomizationNotes,
                UnitPrice = discountedPrice,
                Discount = itemDiscount,
                TotalPrice = totalPrice,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow
            };

            order.Items.Add(orderItem);
        }

        if (subTotal > 0)
        {
            order.SubTotal = subTotal;
            order.Discount = totalDiscount;
            order.TotalAmount = subTotal;
        }

        var systemComment = new OrderComment
        {
            OrderId = order.Id,
            Content = $"Order created with {cart.Items.Count} item(s)" +
                      (discountRate > 0 ? $" · {discountRate}% partner discount applied" : string.Empty),
            Type = CommentType.StatusChange,
            AuthorId = request.PartnerUserId,
            AuthorName = "System",
            AuthorType = "System",
            IsSystemGenerated = true,
            CreatedAt = DateTime.UtcNow
        };

        order.Comments.Add(systemComment);

        _context.Orders.Add(order);

        _context.CartItems.RemoveRange(cart.Items);
        cart.IsActive = false;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(order.Id);
    }
}
