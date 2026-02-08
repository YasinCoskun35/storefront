using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record AddToCartCommand(
    string PartnerUserId,
    string PartnerCompanyId,
    string ProductId,
    string ProductName,
    string ProductSKU,
    string? ProductImageUrl,
    int Quantity,
    string? ColorChartId,
    string? ColorChartName,
    string? ColorOptionId,
    string? ColorOptionName,
    string? ColorOptionCode,
    string? CustomizationNotes
) : IRequest<Result<string>>;
