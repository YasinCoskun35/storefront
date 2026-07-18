using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record RequestPartnerPasswordResetCommand(string Email) : IRequest<Result>;

public class RequestPartnerPasswordResetCommandHandler
    : IRequestHandler<RequestPartnerPasswordResetCommand, Result>
{
    private readonly IdentityDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RequestPartnerPasswordResetCommandHandler> _logger;

    public RequestPartnerPasswordResetCommandHandler(
        IdentityDbContext context,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<RequestPartnerPasswordResetCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result> Handle(RequestPartnerPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _context.PartnerUsers
            .FirstOrDefaultAsync(pu => pu.Email.ToLower() == email && pu.IsActive, cancellationToken);

        // Always succeed — never reveal whether an account exists
        if (user is null)
        {
            _logger.LogInformation("Password reset requested for unknown email");
            return Result.Success();
        }

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToHexString(tokenBytes).ToLowerInvariant();

        user.PasswordResetTokenHash = HashToken(token);
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddHours(1);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var frontendBaseUrl = (_configuration["Frontend:BaseUrl"] ?? "http://localhost:3000").TrimEnd('/');
        var resetLink = $"{frontendBaseUrl}/partner/reset-password?token={token}";

        var body = $"""
            <div style="font-family:Arial,sans-serif;max-width:520px;margin:0 auto">
              <h2>Şifre Sıfırlama / Password Reset</h2>
              <p>Merhaba {user.FirstName},</p>
              <p>Hesabınız için bir şifre sıfırlama talebi aldık. Yeni şifrenizi belirlemek için aşağıdaki bağlantıya tıklayın. Bağlantı 1 saat geçerlidir.</p>
              <p>We received a password reset request for your account. Click the link below to set a new password. The link is valid for 1 hour.</p>
              <p style="margin:24px 0">
                <a href="{resetLink}" style="background:#2563eb;color:#fff;padding:12px 24px;border-radius:6px;text-decoration:none">
                  Şifremi Sıfırla / Reset Password
                </a>
              </p>
              <p style="color:#6b7280;font-size:13px">Bu talebi siz yapmadıysanız bu e-postayı yok sayabilirsiniz.<br/>If you did not request this, you can safely ignore this email.</p>
            </div>
            """;

        await _emailService.SendAsync(user.Email, "Şifre Sıfırlama Talebi — Storefront", body, cancellationToken);

        return Result.Success();
    }

    internal static string HashToken(string token)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
