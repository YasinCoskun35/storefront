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
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.PartnerUserId == request.PartnerUserId, cancellationToken);

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
        else if (!cart.IsActive)
        {
            cart.IsActive = true;
            cart.UpdatedAt = DateTime.UtcNow;
        }

        // Match on product + selected variants combination
        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId &&
            i.SelectedVariants == request.SelectedVariants);

        if (existingItem is not null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                ProductName = request.ProductName,
                ProductSKU = request.ProductSKU,
                ProductImageUrl = request.ProductImageUrl,
                Quantity = request.Quantity,
                SelectedVariants = request.SelectedVariants,
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
