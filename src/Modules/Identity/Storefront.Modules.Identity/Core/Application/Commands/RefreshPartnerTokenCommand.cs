using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record RefreshPartnerTokenCommand(string UserId) : IRequest<Result<PartnerLoginResponse>>;

public class RefreshPartnerTokenCommandHandler : IRequestHandler<RefreshPartnerTokenCommand, Result<PartnerLoginResponse>>
{
    private readonly IdentityDbContext _context;
    private readonly IConfiguration _configuration;

    public RefreshPartnerTokenCommandHandler(IdentityDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<Result<PartnerLoginResponse>> Handle(RefreshPartnerTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.PartnerUsers
            .Include(pu => pu.Company)
            .AsNoTracking()
            .FirstOrDefaultAsync(pu => pu.Id == request.UserId, cancellationToken);

        if (user is null || !user.IsActive)
            return Result<PartnerLoginResponse>.Failure(
                Error.Validation("Partner.Unauthorized", "Session is no longer valid."));

        if (user.Company.Status != PartnerStatus.Active)
            return Result<PartnerLoginResponse>.Failure(
                Error.Validation("Partner.CompanyInactive", "Company account is not active."));

        var jwtSettings = _configuration.GetSection("Jwt");
        var secret    = jwtSettings["Secret"]   ?? throw new InvalidOperationException("JWT Secret is not configured.");
        var issuer    = jwtSettings["Issuer"]   ?? "Storefront.Partner";
        var audience  = jwtSettings["Audience"] ?? "Storefront.Partner.Web";
        const int expiresIn = 1800; // 30 minutes

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,        user.Id),
            new Claim(JwtRegisteredClaimNames.Email,      user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName,  user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim(ClaimTypes.Role, "Partner"),
            new Claim("role",         user.Role.ToString()),
            new Claim("scopes",       user.Scopes ?? string.Empty),
            new Claim("companyId",    user.PartnerCompanyId),
            new Claim("companyName",  user.Company.CompanyName),
            new Claim("type",         "Partner"),
            new Claim("discountRate", user.Company.DiscountRate.ToString(CultureInfo.InvariantCulture)),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddSeconds(expiresIn),
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var response = new PartnerLoginResponse(
            accessToken,
            string.Empty, // refresh tokens are not persisted for partners
            expiresIn,
            new PartnerUserInfo(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role.ToString(),
                new PartnerCompanyInfo(user.Company.Id, user.Company.CompanyName, user.Company.Status.ToString()),
                user.Company.DiscountRate
            )
        );

        return Result<PartnerLoginResponse>.Success(response);
    }
}
