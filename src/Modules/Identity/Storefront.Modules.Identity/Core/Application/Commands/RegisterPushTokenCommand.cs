using MediatR;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record RegisterPushTokenCommand(string UserId, string? PushToken) : IRequest<Result>;

public class RegisterPushTokenCommandHandler : IRequestHandler<RegisterPushTokenCommand, Result>
{
    private readonly IdentityDbContext _context;

    public RegisterPushTokenCommandHandler(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RegisterPushTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.PartnerUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Error.NotFound("User.NotFound", "Partner user not found.");

        user.PushToken = string.IsNullOrWhiteSpace(request.PushToken) ? null : request.PushToken.Trim();
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
