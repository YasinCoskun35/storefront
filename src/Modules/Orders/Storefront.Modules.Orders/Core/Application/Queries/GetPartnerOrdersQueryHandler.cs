using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.Modules.Orders.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public class GetPartnerOrdersQueryHandler : IRequestHandler<GetPartnerOrdersQuery, Result<PartnerOrdersResponse>>
{
    private readonly OrdersDbContext _context;

    public GetPartnerOrdersQueryHandler(OrdersDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PartnerOrdersResponse>> Handle(GetPartnerOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsQueryable();

        // Filter by company unless admin viewing all orders
        if (!request.AdminMode && !string.IsNullOrEmpty(request.PartnerCompanyId))
        {
            query = query.Where(o => o.PartnerCompanyId == request.PartnerCompanyId);
        }
        else if (!request.AdminMode)
        {
            query = query.Where(o => o.PartnerCompanyId == request.PartnerCompanyId);
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<OrderStatus>(request.Status, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderSummaryDto(
                o.Id,
                o.OrderNumber,
                o.Status.ToString(),
                o.Items.Count,
                o.TotalAmount,
                o.Currency,
                o.CreatedAt,
                o.RequestedDeliveryDate,
                false, // TODO: Calculate unread comments
                o.PartnerCompanyId,
                o.PartnerCompanyName
            ))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new PartnerOrdersResponse(
            orders,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages
        );

        return Result<PartnerOrdersResponse>.Success(response);
    }
}
