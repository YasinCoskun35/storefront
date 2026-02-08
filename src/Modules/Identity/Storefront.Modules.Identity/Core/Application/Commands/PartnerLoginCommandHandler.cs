using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public class PartnerLoginCommandHandler : IRequestHandler<PartnerLoginCommand, Result<PartnerLoginResponse>>
{
    private readonly IdentityDbContext _context;
    private readonly IPasswordHasher<PartnerUser> _passwordHasher;
    private readonly IConfiguration _configuration;

    public PartnerLoginCommandHandler(
        IdentityDbContext context,
        IPasswordHasher<PartnerUser> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task<Result<PartnerLoginResponse>> Handle(PartnerLoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await _context.PartnerUsers
            .Include(pu => pu.Company)
            .FirstOrDefaultAsync(pu => pu.Email == request.Email, cancellationToken);

        if (user is null)
        {
            return Error.Validation("Partner.InvalidCredentials", "Invalid email or password");
        }

        // Check if user is locked out
        if (user.IsLockedOut)
        {
            return Error.Validation(
                "Partner.AccountLocked",
                $"Account is locked until {user.LockoutEnd:yyyy-MM-dd HH:mm:ss} UTC");
        }

        // Verify password
        var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            // Increment failed access count
            user.AccessFailedCount++;
            
            // Lock account after 5 failed attempts
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
            }
            
            await _context.SaveChangesAsync(cancellationToken);
            
            return Error.Validation("Partner.InvalidCredentials", "Invalid email or password");
        }

        // Check if user is active
        if (!user.IsActive)
        {
            return Error.Validation(
                "Partner.AccountInactive",
                "Your account is inactive. Please contact support.");
        }

        // Check if company is approved
        if (user.Company.Status != PartnerStatus.Active)
        {
            return Error.Validation(
                "Partner.CompanyNotApproved",
                user.Company.Status == PartnerStatus.Pending
                    ? "Your company registration is pending approval"
                    : "Your company account is suspended. Please contact support.");
        }

        // Reset failed access count
        user.AccessFailedCount = 0;
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        // Generate JWT token
        var accessToken = GenerateAccessToken(user);
        
        // Generate refresh token
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid().ToString(),
            Token = Guid.NewGuid().ToString(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new PartnerLoginResponse(
            accessToken,
            refreshToken.Token,
            1800, // 30 minutes
            new PartnerUserInfo(
                user.Id,
                user.Email,
                user.FirstName,
                user.LastName,
                user.Role.ToString(),
                new PartnerCompanyInfo(
                    user.Company.Id,
                    user.Company.CompanyName,
                    user.Company.Status.ToString()
                )
            )
        );

        return Result<PartnerLoginResponse>.Success(response);
    }

    private string GenerateAccessToken(PartnerUser user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
        var issuer = jwtSettings["Issuer"] ?? "Storefront.Partner";
        var audience = jwtSettings["Audience"] ?? "Storefront.Partner.Web";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
            new Claim("role", user.Role.ToString()),
            new Claim("companyId", user.PartnerCompanyId),
            new Claim("type", "Partner"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
