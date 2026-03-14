using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Storefront.Modules.Identity.Core.Domain.Enums;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record ProcessPartnerPaymentCallbackCommand(
    string Token,
    string? Status
) : IRequest<Result<string>>; // returns redirect URL

public class ProcessPartnerPaymentCallbackCommandHandler
    : IRequestHandler<ProcessPartnerPaymentCallbackCommand, Result<string>>
{
    private readonly IdentityDbContext _context;
    private readonly IConfiguration _config;
    private readonly IMediator _mediator;

    public ProcessPartnerPaymentCallbackCommandHandler(
        IdentityDbContext context,
        IConfiguration config,
        IMediator mediator)
    {
        _context = context;
        _config = config;
        _mediator = mediator;
    }

    public async Task<Result<string>> Handle(
        ProcessPartnerPaymentCallbackCommand request, CancellationToken cancellationToken)
    {
        var frontendResultUrl = _config["Iyzico:FrontendResultUrl"]
            ?? "http://localhost:3000/partner/payments/result";

        var payment = await _context.PartnerPayments
            .FirstOrDefaultAsync(p => p.IyzicoToken == request.Token, cancellationToken);

        if (payment is null)
            return Result<string>.Success($"{frontendResultUrl}?status=failed&reason=not_found");

        if (payment.Status != "Pending")
            return Result<string>.Success($"{frontendResultUrl}?status=already_processed");

        // Verify with iyzico
        var options = BuildOptions();
        var retrieveRequest = new RetrieveCheckoutFormRequest
        {
            Token = request.Token,
            Locale = Locale.TR.ToString(),
        };

        var checkoutForm = CheckoutForm.Retrieve(retrieveRequest, options);

        if (checkoutForm.Status == "success" && checkoutForm.PaymentStatus == "SUCCESS")
        {
            payment.Status = "Success";
            payment.IyzicoPaymentId = checkoutForm.PaymentId;
            payment.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            // Record PaymentCredit on the partner's account
            await _mediator.Send(new RecordAccountTransactionCommand(
                CompanyId: payment.PartnerCompanyId,
                Type: TransactionType.PaymentCredit,
                Amount: payment.Amount,
                PaymentMethod: PaymentMethod.BankTransfer,
                OrderReference: null,
                Notes: $"Online ödeme — iyzico #{checkoutForm.PaymentId}",
                CreatedBy: payment.PartnerUserId
            ), cancellationToken);

            var amountFormatted = payment.Amount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
            return Result<string>.Success(
                $"{frontendResultUrl}?status=success&amount={amountFormatted}");
        }
        else
        {
            payment.Status = "Failed";
            payment.FailureReason = checkoutForm.ErrorMessage ?? request.Status;
            payment.CompletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Result<string>.Success(
                $"{frontendResultUrl}?status=failed&reason={Uri.EscapeDataString(payment.FailureReason ?? "unknown")}");
        }
    }

    private Options BuildOptions() => new()
    {
        ApiKey = _config["Iyzico:ApiKey"] ?? string.Empty,
        SecretKey = _config["Iyzico:SecretKey"] ?? string.Empty,
        BaseUrl = _config["Iyzico:BaseUrl"] ?? "https://sandbox-api.iyzipay.com",
    };
}
