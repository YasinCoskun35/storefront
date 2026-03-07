using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public class GetCartQueryHandler : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    private readonly OrdersDbContext _context;

    public GetCartQueryHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.PartnerUserId == request.PartnerUserId && c.IsActive, cancellationToken);

        if (cart is null)
        {
            return Result<CartDto>.Success(new CartDto(
                Guid.NewGuid().ToString(),
                0,
                new List<CartItemDto>()
            ));
        }

        var dto = new CartDto(
            cart.Id,
            cart.Items.Count,
            cart.Items.Select(i => new CartItemDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.ProductSKU,
                i.ProductImageUrl,
                i.Quantity,
                i.SelectedVariants,
                i.CustomizationNotes
            )).ToList()
        );

        return Result<CartDto>.Success(dto);
    }
}
