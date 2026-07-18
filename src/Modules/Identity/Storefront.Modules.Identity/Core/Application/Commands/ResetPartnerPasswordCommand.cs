using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record ResetPartnerPasswordCommand(string Token, string NewPassword) : IRequest<Result>;

public class ResetPartnerPasswordCommandValidator : AbstractValidator<ResetPartnerPasswordCommand>
{
    public ResetPartnerPasswordCommandValidator()
    {
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}

public class ResetPartnerPasswordCommandHandler : IRequestHandler<ResetPartnerPasswordCommand, Result>
{
    private readonly IdentityDbContext _context;
    private readonly IPasswordHasher<PartnerUser> _passwordHasher;

    public ResetPartnerPasswordCommandHandler(
        IdentityDbContext context,
        IPasswordHasher<PartnerUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result> Handle(ResetPartnerPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = RequestPartnerPasswordResetCommandHandler.HashToken(request.Token);

        var user = await _context.PartnerUsers
            .FirstOrDefaultAsync(pu => pu.PasswordResetTokenHash == tokenHash && pu.IsActive, cancellationToken);

        if (user is null || !user.PasswordResetTokenExpiresAt.HasValue
            || user.PasswordResetTokenExpiresAt.Value < DateTime.UtcNow)
        {
            return Result.Failure(Error.Validation(
                "Partner.InvalidResetToken",
                "Reset link is invalid or has expired. Please request a new one."));
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAt = null;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
