using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed record UpdatePartnerUserCommand(
    string UserId,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    bool IsActive
) : IRequest<Result>;

public class UpdatePartnerUserCommandHandler : IRequestHandler<UpdatePartnerUserCommand, Result>
{
    private readonly IdentityDbContext _context;

    public UpdatePartnerUserCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdatePartnerUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.PartnerUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Error.NotFound("PartnerUser.NotFound", "Partner user not found.");

        if (!Enum.TryParse<PartnerRole>(request.Role, out var role))
            return Error.Validation("PartnerUser.InvalidRole", "Invalid role. Use 'User' or 'CompanyAdmin'.");

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Phone = request.Phone;
        user.Role = role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
