using MediatR;
using Storefront.Modules.Catalog.Core.Application.DTOs;
using Storefront.SharedKernel;

namespace Storefront.Modules.Catalog.Core.Application.Queries;

public sealed record GetCategoriesQuery(
    string? ParentId = null,
    bool? IsActive = true
) : IRequest<Result<IReadOnlyList<CategoryDto>>>;

