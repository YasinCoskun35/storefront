using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Core.Application.DTOs;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Queries;

public sealed class GetPageBySlugQueryHandler : IRequestHandler<GetPageBySlugQuery, Result<StaticPageDto>>
{
    private readonly ContentDbContext _context;

    public GetPageBySlugQueryHandler(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StaticPageDto>> Handle(GetPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var page = await _context.StaticPages
            .FirstOrDefaultAsync(sp => sp.Slug == request.Slug && sp.IsPublished, cancellationToken);

        if (page is null)
        {
            return Result<StaticPageDto>.Failure(Error.NotFound("StaticPage.NotFound", $"Page with slug '{request.Slug}' not found."));
        }

        var pageDto = new StaticPageDto(
            page.Id,
            page.Title,
            page.Slug,
            page.Body,
            page.IsPublished
        );

        return Result<StaticPageDto>.Success(pageDto);
    }
}

