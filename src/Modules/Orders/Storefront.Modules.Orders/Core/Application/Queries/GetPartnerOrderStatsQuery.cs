using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public record PartnerOrderStatsDto(
    int TotalOrders,
    int PendingOrders,
    int ActiveOrders,
    int CompletedOrders
);

public record GetPartnerOrderStatsQuery(string PartnerCompanyId) : IRequest<Result<PartnerOrderStatsDto>>;

public class GetPartnerOrderStatsQueryHandler : IRequestHandler<GetPartnerOrderStatsQuery, Result<PartnerOrderStatsDto>>
{
    private readonly OrdersDbContext _context;

    public GetPartnerOrderStatsQueryHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PartnerOrderStatsDto>> Handle(GetPartnerOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var orders = _context.Orders.Where(o => o.PartnerCompanyId == request.PartnerCompanyId);

        var total = await orders.CountAsync(cancellationToken);
        var pending = await orders.CountAsync(o => o.Status == OrderStatus.Pending || o.Status == OrderStatus.QuoteSent, cancellationToken);
        var active = await orders.CountAsync(o =>
            o.Status == OrderStatus.Confirmed ||
            o.Status == OrderStatus.Preparing ||
            o.Status == OrderStatus.QualityCheck ||
            o.Status == OrderStatus.ReadyToShip ||
            o.Status == OrderStatus.Shipping, cancellationToken);
        var completed = await orders.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);

        return Result<PartnerOrderStatsDto>.Success(new PartnerOrderStatsDto(total, pending, active, completed));
    }
}
