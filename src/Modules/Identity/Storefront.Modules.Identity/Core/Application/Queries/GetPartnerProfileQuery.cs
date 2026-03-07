using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Queries;

public record PartnerProfileDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? JobTitle,
    string? PhoneNumber,
    bool IsAdmin,
    PartnerCompanyProfileDto Company
);

public record PartnerCompanyProfileDto(
    string Id,
    string Name,
    string? TaxNumber,
    string? Email,
    string? Phone,
    string? Address,
    string? City,
    string? Country,
    string Status
);

public record GetPartnerProfileQuery(string PartnerUserId) : IRequest<Result<PartnerProfileDto>>;

public class GetPartnerProfileQueryHandler : IRequestHandler<GetPartnerProfileQuery, Result<PartnerProfileDto>>
{
    private readonly IdentityDbContext _context;

    public GetPartnerProfileQueryHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PartnerProfileDto>> Handle(GetPartnerProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.PartnerUsers
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == request.PartnerUserId, cancellationToken);

        if (user is null)
            return Result<PartnerProfileDto>.Failure(Error.NotFound("PartnerUser.NotFound", "User not found."));

        var dto = new PartnerProfileDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            null,
            user.Phone,
            user.Role == Core.Domain.Enums.PartnerRole.CompanyAdmin,
            new PartnerCompanyProfileDto(
                user.Company.Id,
                user.Company.CompanyName,
                user.Company.TaxId,
                user.Company.Email,
                user.Company.Phone,
                user.Company.Address,
                user.Company.City,
                user.Company.Country,
                user.Company.Status.ToString()
            )
        );

        return Result<PartnerProfileDto>.Success(dto);
    }
}
