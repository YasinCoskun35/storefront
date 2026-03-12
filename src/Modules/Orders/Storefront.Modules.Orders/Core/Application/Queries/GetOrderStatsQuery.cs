using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public record OrderStatsDto(
    int TotalOrders,
    int PendingOrders,
    int ActiveOrders,
    int CompletedOrders,
    int TotalPartners
);

public record GetOrderStatsQuery : IRequest<Result<OrderStatsDto>>;

public class GetOrderStatsQueryHandler : IRequestHandler<GetOrderStatsQuery, Result<OrderStatsDto>>
{
    private readonly OrdersDbContext _context;

    public GetOrderStatsQueryHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<OrderStatsDto>> Handle(GetOrderStatsQuery request, CancellationToken cancellationToken)
    {
        var total = await _context.Orders.CountAsync(cancellationToken);
        var pending = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var active = await _context.Orders.CountAsync(o =>
            o.Status == OrderStatus.Confirmed ||
            o.Status == OrderStatus.Preparing ||
            o.Status == OrderStatus.QualityCheck ||
            o.Status == OrderStatus.ReadyToShip ||
            o.Status == OrderStatus.Shipping, cancellationToken);
        var completed = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);
        var partners = await _context.Orders.Select(o => o.PartnerCompanyId).Distinct().CountAsync(cancellationToken);

        return Result<OrderStatsDto>.Success(new OrderStatsDto(total, pending, active, completed, partners));
    }
}
