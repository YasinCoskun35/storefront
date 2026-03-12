using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public record RemoveCartItemCommand(string UserId, string CartItemId) : IRequest<Result>;
