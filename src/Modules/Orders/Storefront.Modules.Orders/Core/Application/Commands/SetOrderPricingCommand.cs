using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record OrderItemPricingDto(string OrderItemId, decimal UnitPrice, decimal? Discount);

public record SetOrderPricingCommand(
    string OrderId,
    List<OrderItemPricingDto> ItemPricing,
    decimal? ShippingCost,
    decimal? TaxAmount,
    decimal? Discount,
    string? Currency,
    string? Notes
) : IRequest<Result>;
