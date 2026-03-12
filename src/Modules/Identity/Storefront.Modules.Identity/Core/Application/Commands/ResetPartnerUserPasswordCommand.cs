using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed record ResetPartnerUserPasswordCommand(
    string UserId,
    string NewPassword
) : IRequest<Result>;

public class ResetPartnerUserPasswordCommandHandler : IRequestHandler<ResetPartnerUserPasswordCommand, Result>
{
    private readonly IdentityDbContext _context;
    private readonly IPasswordHasher<PartnerUser> _passwordHasher;

    public ResetPartnerUserPasswordCommandHandler(
        IdentityDbContext context,
        IPasswordHasher<PartnerUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ResetPartnerUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.PartnerUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Error.NotFound("PartnerUser.NotFound", "Partner user not found.");

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
