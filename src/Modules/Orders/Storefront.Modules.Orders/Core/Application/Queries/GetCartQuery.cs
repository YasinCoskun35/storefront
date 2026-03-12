using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Queries;

public record GetCartQuery(string PartnerUserId) : IRequest<Result<CartDto>>;

public record CartDto(
    string Id,
    int ItemCount,
    List<CartItemDto> Items
);

public record CartItemDto(
    string Id,
    string ProductId,
    string ProductName,
    string ProductSKU,
    string? ProductImageUrl,
    int Quantity,
    decimal? UnitPrice,
    string? SelectedVariants,
    string? CustomizationNotes
);
