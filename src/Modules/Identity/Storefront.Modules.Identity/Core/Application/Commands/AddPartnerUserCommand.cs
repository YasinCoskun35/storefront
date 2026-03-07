using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed record AddPartnerUserCommand(
    string CompanyId,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role
) : IRequest<Result<string>>;

public class AddPartnerUserCommandHandler : IRequestHandler<AddPartnerUserCommand, Result<string>>
{
    private readonly IdentityDbContext _context;
    private readonly IPasswordHasher<PartnerUser> _passwordHasher;

    public AddPartnerUserCommandHandler(
        IdentityDbContext context,
        IPasswordHasher<PartnerUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<string>> Handle(AddPartnerUserCommand request, CancellationToken cancellationToken)
    {
        var company = await _context.PartnerCompanies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, cancellationToken);

        if (company is null)
            return Error.NotFound("Partner.NotFound", "Partner company not found.");

        var existingUser = await _context.PartnerUsers
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (existingUser is not null)
            return Error.Conflict("Partner.EmailAlreadyExists", $"A user with email '{request.Email}' already exists.");

        if (!Enum.TryParse<PartnerRole>(request.Role, out var role))
            role = PartnerRole.User;

        var user = new PartnerUser
        {
            PartnerCompanyId = request.CompanyId,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = role,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        _context.PartnerUsers.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(user.Id);
    }
}
