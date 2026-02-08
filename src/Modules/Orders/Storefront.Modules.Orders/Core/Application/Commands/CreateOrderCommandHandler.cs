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

    public CreateOrderCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Get partner's cart
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.PartnerUserId == request.PartnerUserId && c.IsActive, cancellationToken);

        if (cart is null || !cart.Items.Any())
        {
            return Error.Validation("Order.EmptyCart", "Cannot create order from empty cart");
        }

        // Generate order number
        var orderCount = await _context.Orders.CountAsync(cancellationToken);
        var orderNumber = $"ORD-{DateTime.UtcNow:yyyy}-{(orderCount + 1):D4}";

        // Get partner company name (would normally query Identity module, but denormalizing for simplicity)
        var partnerCompanyName = "Partner Company"; // TODO: Get from Identity module

        // Create order
        var order = new Order
        {
            OrderNumber = orderNumber,
            PartnerCompanyId = request.PartnerCompanyId,
            PartnerUserId = request.PartnerUserId,
            PartnerCompanyName = partnerCompanyName,
            Status = OrderStatus.Pending,
            DeliveryAddress = request.DeliveryAddress,
            DeliveryCity = request.DeliveryCity,
            DeliveryState = request.DeliveryState,
            DeliveryPostalCode = request.DeliveryPostalCode,
            DeliveryCountry = request.DeliveryCountry,
            DeliveryNotes = request.DeliveryNotes,
            RequestedDeliveryDate = request.RequestedDeliveryDate,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow
        };

        // Convert cart items to order items
        foreach (var cartItem in cart.Items)
        {
            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.ProductName,
                ProductSKU = cartItem.ProductSKU,
                ProductImageUrl = cartItem.ProductImageUrl,
                Quantity = cartItem.Quantity,
                ColorChartId = cartItem.ColorChartId,
                ColorChartName = cartItem.ColorChartName,
                ColorOptionId = cartItem.ColorOptionId,
                ColorOptionName = cartItem.ColorOptionName,
                ColorOptionCode = cartItem.ColorOptionCode,
                CustomizationNotes = cartItem.CustomizationNotes,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow
            };

            order.Items.Add(orderItem);
        }

        // Add system comment
        var systemComment = new OrderComment
        {
            OrderId = order.Id,
            Content = $"Order created with {cart.Items.Count} item(s)",
            Type = CommentType.StatusChange,
            AuthorId = request.PartnerUserId,
            AuthorName = "System",
            AuthorType = "System",
            IsSystemGenerated = true,
            CreatedAt = DateTime.UtcNow
        };

        order.Comments.Add(systemComment);

        _context.Orders.Add(order);

        // Clear cart
        cart.IsActive = false;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(order.Id);
    }
}
