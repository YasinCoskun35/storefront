using MediatR;
using Storefront.Modules.Orders.Core.Domain.Enums;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record UpdateOrderStatusCommand(
    string OrderId,
    OrderStatus NewStatus,
    string UpdatedBy, // Admin user ID
    string UpdatedByName,
    string? Notes
) : IRequest<Result>;
