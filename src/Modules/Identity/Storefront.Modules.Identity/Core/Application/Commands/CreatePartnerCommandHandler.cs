using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public class CreatePartnerCommandHandler : IRequestHandler<CreatePartnerCommand, Result<string>>
{
    private readonly IdentityDbContext _context;
    private readonly IPasswordHasher<PartnerUser> _passwordHasher;

    public CreatePartnerCommandHandler(
        IdentityDbContext context,
        IPasswordHasher<PartnerUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<string>> Handle(CreatePartnerCommand request, CancellationToken cancellationToken)
    {
        // Check if company with same Tax ID already exists
        var existingCompany = await _context.PartnerCompanies
            .FirstOrDefaultAsync(pc => pc.TaxId == request.TaxId, cancellationToken);

        if (existingCompany is not null)
        {
            return Error.Conflict(
                "Partner.TaxIdAlreadyExists",
                $"A company with Tax ID '{request.TaxId}' is already registered");
        }

        // Check if user with same email already exists
        var existingUser = await _context.PartnerUsers
            .FirstOrDefaultAsync(pu => pu.Email == request.AdminUser.Email, cancellationToken);

        if (existingUser is not null)
        {
            return Error.Conflict(
                "Partner.EmailAlreadyExists",
                $"A user with email '{request.AdminUser.Email}' already exists");
        }

        // Create partner company - ACTIVE by default (created by admin)
        var company = new PartnerCompany
        {
            CompanyName = request.CompanyName,
            TaxId = request.TaxId,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PostalCode = request.PostalCode,
            Country = request.Country,
            Industry = request.Industry,
            Website = request.Website,
            EmployeeCount = request.EmployeeCount,
            AnnualRevenue = request.AnnualRevenue,
            Status = PartnerStatus.Active, // Active immediately (created by admin)
            CreatedAt = DateTime.UtcNow,
            ApprovedAt = DateTime.UtcNow, // Auto-approved
            ApprovedBy = request.CreatedByAdminId,
            ApprovalNotes = "Created by admin"
        };

        _context.PartnerCompanies.Add(company);

        // Create admin user - ACTIVE immediately
        var adminUser = new PartnerUser
        {
            PartnerCompanyId = company.Id,
            Email = request.AdminUser.Email,
            FirstName = request.AdminUser.FirstName,
            LastName = request.AdminUser.LastName,
            Role = PartnerRole.CompanyAdmin,
            IsActive = true, // Active immediately (created by admin)
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password
        adminUser.PasswordHash = _passwordHasher.HashPassword(adminUser, request.AdminUser.Password);

        _context.PartnerUsers.Add(adminUser);

        // Save changes
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(company.Id);
    }
}
