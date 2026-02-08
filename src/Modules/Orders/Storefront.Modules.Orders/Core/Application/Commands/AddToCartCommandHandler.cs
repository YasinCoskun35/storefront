using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Entities;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, Result<string>>
{
    private readonly OrdersDbContext _context;

    public AddToCartCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<string>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        // Get or create cart for this partner user
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.PartnerUserId == request.PartnerUserId && c.IsActive, cancellationToken);

        if (cart is null)
        {
            cart = new Cart
            {
                PartnerUserId = request.PartnerUserId,
                PartnerCompanyId = request.PartnerCompanyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
        }

        // Check if same product + color already in cart
        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId &&
            i.ColorOptionId == request.ColorOptionId);

        if (existingItem is not null)
        {
            // Update quantity instead of adding duplicate
            existingItem.Quantity += request.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Add new item
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                ProductName = request.ProductName,
                ProductSKU = request.ProductSKU,
                ProductImageUrl = request.ProductImageUrl,
                Quantity = request.Quantity,
                ColorChartId = request.ColorChartId,
                ColorChartName = request.ColorChartName,
                ColorOptionId = request.ColorOptionId,
                ColorOptionName = request.ColorOptionName,
                ColorOptionCode = request.ColorOptionCode,
                CustomizationNotes = request.CustomizationNotes,
                CreatedAt = DateTime.UtcNow
            };

            cart.Items.Add(cartItem);
        }

        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(cart.Id);
    }
}
