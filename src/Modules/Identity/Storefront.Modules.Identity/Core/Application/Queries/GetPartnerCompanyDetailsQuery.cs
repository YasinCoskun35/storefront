using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Queries;

public record GetPartnerCompanyDetailsQuery(string CompanyId) : IRequest<Result<PartnerCompanyDetailsDto>>;

public record PartnerCompanyDetailsDto(
    string Id,
    string CompanyName,
    string TaxId,
    string Email,
    string Phone,
    string Address,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? Industry,
    string? Website,
    int? EmployeeCount,
    decimal? AnnualRevenue,
    string? Notes,
    string Status,
    DateTime CreatedAt,
    DateTime? ApprovedAt,
    string? ApprovedBy,
    string? ApprovalNotes,
    List<PartnerUserDto> Users,
    List<PartnerContactDto> Contacts
);

public record PartnerUserDto(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record PartnerContactDto(
    string Id,
    string Name,
    string Title,
    string Email,
    string Phone,
    bool IsPrimary
);
