using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public class SuspendPartnerCommandHandler : IRequestHandler<SuspendPartnerCommand, Result>
{
    private readonly IdentityDbContext _context;

    public SuspendPartnerCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(SuspendPartnerCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.PartnerCompanies
            .Include(pc => pc.Users)
            .FirstOrDefaultAsync(pc => pc.Id == request.PartnerCompanyId, cancellationToken);

        if (company is null)
        {
            return Error.NotFound("Partner.NotFound", "Partner company not found");
        }

        if (company.Status == PartnerStatus.Suspended)
        {
            return Error.Validation("Partner.AlreadySuspended", "Partner company is already suspended");
        }

        // Update company status
        company.Status = PartnerStatus.Suspended;
        company.ApprovalNotes = request.Reason;
        company.UpdatedAt = DateTime.UtcNow;

        // Deactivate all company users
        foreach (var user in company.Users)
        {
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        // TODO: Send suspension email to partner admin

        return Result.Success();
    }
}
