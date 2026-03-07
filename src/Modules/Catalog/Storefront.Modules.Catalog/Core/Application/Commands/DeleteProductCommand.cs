using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record DeleteProductCommand(string ProductId) : IRequest<Result>;
