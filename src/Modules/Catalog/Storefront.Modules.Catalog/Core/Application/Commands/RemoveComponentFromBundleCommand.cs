using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record RemoveComponentFromBundleCommand(
    string BundleProductId,
    string ComponentProductId
) : IRequest<Result<bool>>;
