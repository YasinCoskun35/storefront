using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record UpdatePartnerPricingCommand(string CompanyId, decimal DiscountRate) : IRequest<Result>;

public class UpdatePartnerPricingCommandHandler : IRequestHandler<UpdatePartnerPricingCommand, Result>
{
    private readonly IdentityDbContext _context;

    public UpdatePartnerPricingCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdatePartnerPricingCommand request, CancellationToken cancellationToken)
    {
        if (request.DiscountRate < 0 || request.DiscountRate > 100)
        {
            return Error.Validation("Partner.InvalidDiscountRate", "Discount rate must be between 0 and 100.");
        }

        var company = await _context.PartnerCompanies
            .FirstOrDefaultAsync(pc => pc.Id == request.CompanyId, cancellationToken);

        if (company is null)
        {
            return Error.NotFound("Partner.NotFound", "Partner company not found.");
        }

        company.DiscountRate = request.DiscountRate;
        company.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
