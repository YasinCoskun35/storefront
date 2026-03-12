using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Content.Core.Application.DTOs;
using Storefront.Modules.Content.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Content.Core.Application.Queries;

public sealed class GetAppSettingsQueryHandler : IRequestHandler<GetAppSettingsQuery, Result<List<AppSettingDto>>>
{
    private readonly ContentDbContext _context;

    public GetAppSettingsQueryHandler(ContentDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AppSettingDto>>> Handle(GetAppSettingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AppSettings.AsQueryable();

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(s => s.Category == request.Category);
        }

        var settings = await query
            .OrderBy(s => s.Category)
            .ThenBy(s => s.DisplayName)
            .Select(s => new AppSettingDto(
                s.Key,
                s.Value,
                s.DisplayName,
                s.Description,
                s.Category,
                s.DataType,
                s.UpdatedAt,
                s.UpdatedBy
            ))
            .ToListAsync(cancellationToken);

        return Result<List<AppSettingDto>>.Success(settings);
    }
}
