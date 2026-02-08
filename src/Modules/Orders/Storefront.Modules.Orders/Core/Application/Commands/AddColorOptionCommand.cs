using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record AddColorOptionCommand(
    string ColorChartId,
    string Name,
    string Code,
    string? HexColor,
    string? ImageUrl,
    decimal? PriceAdjustment,
    int DisplayOrder
) : IRequest<Result<string>>;
