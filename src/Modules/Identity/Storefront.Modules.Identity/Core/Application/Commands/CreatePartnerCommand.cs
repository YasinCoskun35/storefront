using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record CreatePartnerCommand(
    // Company Information
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
    
    // Admin User
    AdminUserInfo AdminUser,
    
    // Created by admin
    string CreatedByAdminId
) : IRequest<Result<string>>;

public record AdminUserInfo(
    string FirstName,
    string LastName,
    string Email,
    string Password
);
