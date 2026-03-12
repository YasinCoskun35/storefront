using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public class GetOrderDetailsQueryHandler : IRequestHandler<GetOrderDetailsQuery, Result<OrderDetailsDto>>
{
    private readonly OrdersDbContext _context;

    public GetOrderDetailsQueryHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OrderDetailsDto>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.Comments)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order is null)
        {
            return Error.NotFound("Order.NotFound", "Order not found");
        }

        var dto = new OrderDetailsDto(
            order.Id,
            order.OrderNumber,
            order.Status.ToString(),
            order.PartnerCompanyId,
            order.PartnerCompanyName,
            order.SubTotal,
            order.TaxAmount,
            order.ShippingCost,
            order.Discount,
            order.TotalAmount,
            order.Currency,
            order.DeliveryAddress,
            order.DeliveryCity,
            order.DeliveryState,
            order.DeliveryPostalCode,
            order.DeliveryCountry,
            order.DeliveryNotes,
            order.RequestedDeliveryDate,
            order.ExpectedDeliveryDate,
            order.TrackingNumber,
            order.ShippingProvider,
            order.Notes,
            order.CreatedAt,
            order.SubmittedAt,
            order.ConfirmedAt,
            order.Items.OrderBy(i => i.DisplayOrder).Select(i => new OrderItemDetailsDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.ProductSKU,
                i.ProductImageUrl,
                i.Quantity,
                i.SelectedVariants,
                i.UnitPrice,
                i.Discount,
                i.TotalPrice,
                i.CustomizationNotes
            )).ToList(),
            order.Comments
                .Where(c => request.IncludeInternalComments || !c.IsInternal)
                .OrderBy(c => c.CreatedAt).Select(c => new OrderCommentDto(
                c.Id,
                c.Content,
                c.Type.ToString(),
                c.AuthorName,
                c.AuthorType,
                c.IsInternal,
                c.AttachmentUrl,
                c.AttachmentFileName,
                c.CreatedAt
            )).ToList()
        );

        return Result<OrderDetailsDto>.Success(dto);
    }
}
