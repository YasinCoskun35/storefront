using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, Result>
{
    private readonly OrdersDbContext _context;

    public RemoveCartItemCommandHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.PartnerUserId == request.UserId && c.IsActive, cancellationToken);

        if (cart is null)
            return Result.Failure(Error.NotFound("Cart.NotFound", "Cart not found."));

        var item = cart.Items.FirstOrDefault(i => i.Id == request.CartItemId);
        if (item is null)
            return Result.Failure(Error.NotFound("CartItem.NotFound", "Cart item not found."));

        _context.CartItems.Remove(item);
        cart.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
