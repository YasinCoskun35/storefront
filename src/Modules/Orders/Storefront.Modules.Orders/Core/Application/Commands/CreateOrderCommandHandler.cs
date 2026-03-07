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
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.PartnerUserId == request.PartnerUserId && c.IsActive, cancellationToken);

        if (cart is null || !cart.Items.Any())
        {
            return Error.Validation("Order.EmptyCart", "Cannot create order from empty cart");
        }

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
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = DateTime.UtcNow
        };

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
                SelectedVariants = cartItem.SelectedVariants,
                CustomizationNotes = cartItem.CustomizationNotes,
                DisplayOrder = 0,
                CreatedAt = DateTime.UtcNow
            };

            order.Items.Add(orderItem);
        }

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

        _context.CartItems.RemoveRange(cart.Items);
        cart.IsActive = false;
        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(order.Id);
    }
}
