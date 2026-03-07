using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand, Result>
{
    private readonly OrdersDbContext _context;

    public UpdateCartItemQuantityCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity < 1)
            return Result.Failure(Error.Validation("Quantity.Invalid", "Quantity must be at least 1."));

        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.PartnerUserId == request.UserId && c.IsActive, cancellationToken);

        if (cart is null)
            return Result.Failure(Error.NotFound("Cart.NotFound", "Cart not found."));

        var item = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId);
        if (item is null)
            return Result.Failure(Error.NotFound("CartItem.NotFound", "Cart item not found."));

        item.Quantity = request.Quantity;
        item.UpdatedAt = DateTime.UtcNow;
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
