using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Queries;

public class GetPartnerCompanyDetailsQueryHandler : IRequestHandler<GetPartnerCompanyDetailsQuery, Result<PartnerCompanyDetailsDto>>
{
    private readonly IdentityDbContext _context;

    public GetPartnerCompanyDetailsQueryHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PartnerCompanyDetailsDto>> Handle(GetPartnerCompanyDetailsQuery request, CancellationToken cancellationToken)
    {
        var company = await _context.PartnerCompanies
            .Include(pc => pc.Users)
            .Include(pc => pc.Contacts)
            .Include(pc => pc.AccountTransactions)
            .FirstOrDefaultAsync(pc => pc.Id == request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Error.NotFound("Partner.NotFound", "Partner company not found");
        }

        var dto = new PartnerCompanyDetailsDto(
            company.Id,
            company.CompanyName,
            company.TaxId,
            company.Email,
            company.Phone,
            company.Address,
            company.City,
            company.State,
            company.PostalCode,
            company.Country,
            company.Industry,
            company.Website,
            company.EmployeeCount,
            company.AnnualRevenue,
            company.Notes,
            company.Status.ToString(),
            company.CreatedAt,
            company.ApprovedAt,
            company.ApprovedBy,
            company.ApprovalNotes,
            company.Users.Select(u => new PartnerUserDto(
                u.Id,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Role.ToString(),
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt,
                u.GetScopesList()
            )).ToList(),
            company.Contacts.Where(c => c.IsActive).Select(c => new PartnerContactDto(
                c.Id,
                c.Name,
                c.Title,
                c.Email,
                c.Phone,
                c.IsPrimary
            )).ToList(),
            company.DiscountRate,
            company.CurrentBalance,
            company.AccountTransactions
                .OrderByDescending(t => t.CreatedAt)
                .Take(50)
                .Select(t => new PartnerAccountTransactionDto(
                    t.Id,
                    t.Type.ToString(),
                    t.Amount,
                    t.PaymentMethod.HasValue ? t.PaymentMethod.Value.ToString() : null,
                    t.OrderReference,
                    t.Notes,
                    t.CreatedBy,
                    t.CreatedAt
                )).ToList()
        );

        return Result<PartnerCompanyDetailsDto>.Success(dto);
    }
}
