using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public record GetPartnerOrdersQuery(
    string PartnerCompanyId,
    string? Status,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PartnerOrdersResponse>>;

public record PartnerOrdersResponse(
    List<OrderSummaryDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public record OrderSummaryDto(
    string Id,
    string OrderNumber,
    string Status,
    int ItemCount,
    decimal? TotalAmount,
    string? Currency,
    DateTime CreatedAt,
    DateTime? RequestedDeliveryDate,
    bool HasUnreadComments
);
