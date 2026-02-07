using MediatR;
using Storefront.Modules.Content.Core.Application.DTOs;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Queries;

public sealed record GetPageBySlugQuery(string Slug) : IRequest<Result<StaticPageDto>>;

