using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record UpdateCartItemQuantityCommand(string UserId, string CartItemId, int Quantity) : IRequest<Result>;
