using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public record GetOrderDetailsQuery(string OrderId, bool IncludeInternalComments = true) : IRequest<Result<OrderDetailsDto>>;

public record OrderDetailsDto(
    string Id,
    string OrderNumber,
    string Status,
    string PartnerCompanyId,
    string PartnerCompanyName,
    decimal? SubTotal,
    decimal? TaxAmount,
    decimal? ShippingCost,
    decimal? Discount,
    decimal? TotalAmount,
    string? Currency,
    string DeliveryAddress,
    string DeliveryCity,
    string DeliveryState,
    string DeliveryPostalCode,
    string DeliveryCountry,
    string? DeliveryNotes,
    DateTime? RequestedDeliveryDate,
    DateTime? ExpectedDeliveryDate,
    string? TrackingNumber,
    string? ShippingProvider,
    string? Notes,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? ConfirmedAt,
    List<OrderItemDetailsDto> Items,
    List<OrderCommentDto> Comments
);

public record OrderItemDetailsDto(
    string Id,
    string ProductId,
    string ProductName,
    string ProductSKU,
    string? ProductImageUrl,
    int Quantity,
    string? SelectedVariants,
    decimal? UnitPrice,
    decimal? Discount,
    decimal? TotalPrice,
    string? CustomizationNotes
);

public record OrderCommentDto(
    string Id,
    string Content,
    string Type,
    string AuthorName,
    string AuthorType,
    bool IsInternal,
    string? AttachmentUrl,
    string? AttachmentFileName,
    DateTime CreatedAt
);
