using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record UpdateCategoryCommand(
    string Id,
    string Name,
    string? Description,
    string? Slug,
    string? ParentId,
    int DisplayOrder,
    bool IsActive,
    bool ShowInNavbar
) : IRequest<Result>;
