using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public class ApprovePartnerCommandHandler : IRequestHandler<ApprovePartnerCommand, Result>
{
    private readonly IdentityDbContext _context;

    public ApprovePartnerCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(ApprovePartnerCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.PartnerCompanies
            .Include(pc => pc.Users)
            .FirstOrDefaultAsync(pc => pc.Id == request.PartnerCompanyId, cancellationToken);

        if (company is null)
        {
            return Error.NotFound("Partner.NotFound", "Partner company not found");
        }

        if (company.Status == PartnerStatus.Active)
        {
            return Error.Validation("Partner.AlreadyApproved", "Partner company is already approved");
        }

        // Update company status
        company.Status = PartnerStatus.Active;
        company.ApprovedAt = DateTime.UtcNow;
        company.ApprovedBy = request.AdminUserId;
        company.ApprovalNotes = request.ApprovalNotes;
        company.UpdatedAt = DateTime.UtcNow;

        // Activate all company users
        foreach (var user in company.Users)
        {
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Send approval email to partner admin

        return Result.Success();
    }
}
