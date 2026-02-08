using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Queries;

public record GetPartnerCompaniesQuery(
    string? SearchTerm,
    string? Status,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PartnerCompaniesResponse>>;

public record PartnerCompaniesResponse(
    List<PartnerCompanyDto> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public record PartnerCompanyDto(
    string Id,
    string CompanyName,
    string TaxId,
    string Email,
    string Phone,
    string City,
    string State,
    string Country,
    string Status,
    int UserCount,
    DateTime CreatedAt,
    DateTime? ApprovedAt
);
