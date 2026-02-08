using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record CreateOrderCommand(
    string PartnerUserId,
    string PartnerCompanyId,
    string DeliveryAddress,
    string DeliveryCity,
    string DeliveryState,
    string DeliveryPostalCode,
    string DeliveryCountry,
    string? DeliveryNotes,
    DateTime? RequestedDeliveryDate,
    string? Notes
) : IRequest<Result<string>>;
