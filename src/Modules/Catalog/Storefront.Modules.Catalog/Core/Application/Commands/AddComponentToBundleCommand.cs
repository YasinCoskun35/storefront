using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record AddComponentToBundleCommand(
    string BundleProductId,
    string ComponentProductId,
    int Quantity,
    decimal? PriceOverride = null,
    bool IsOptional = false,
    int DisplayOrder = 0
) : IRequest<Result<string>>;
