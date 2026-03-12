using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Commands;

public sealed record CreateCategoryCommand(
    string Name,
    string? Description,
    string? Slug,
    string? ParentId,
    int DisplayOrder = 0,
    bool IsActive = true,
    bool ShowInNavbar = false
) : IRequest<Result<string>>;



