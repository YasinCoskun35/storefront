using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Queries;

public class GetPartnerCompaniesQueryHandler : IRequestHandler<GetPartnerCompaniesQuery, Result<PartnerCompaniesResponse>>
{
    private readonly IdentityDbContext _context;

    public GetPartnerCompaniesQueryHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PartnerCompaniesResponse>> Handle(GetPartnerCompaniesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PartnerCompanies
            .Include(pc => pc.Users)
            .AsQueryable();

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(pc =>
                pc.CompanyName.ToLower().Contains(searchTerm) ||
                pc.TaxId.ToLower().Contains(searchTerm) ||
                pc.Email.ToLower().Contains(searchTerm));
        }

        // Filter by status
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<PartnerStatus>(request.Status, out var status))
        {
            query = query.Where(pc => pc.Status == status);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .OrderByDescending(pc => pc.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(pc => new PartnerCompanyDto(
                pc.Id,
                pc.CompanyName,
                pc.TaxId,
                pc.Email,
                pc.Phone,
                pc.City,
                pc.State,
                pc.Country,
                pc.Status.ToString(),
                pc.Users.Count,
                pc.CreatedAt,
                pc.ApprovedAt
            ))
            .ToListAsync(cancellationToken);

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new PartnerCompaniesResponse(
            items,
            request.PageNumber,
            request.PageSize,
            totalCount,
            totalPages
        );

        return Result<PartnerCompaniesResponse>.Success(response);
    }
}
