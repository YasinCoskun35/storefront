using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record CreateColorChartCommand(
    string Name,
    string Code,
    string Description,
    string Type, // "Fabric", "Leather", "Wood", "Metal"
    string? MainImageUrl,
    string? ThumbnailUrl,
    string CreatedBy // Admin user ID
) : IRequest<Result<string>>;
