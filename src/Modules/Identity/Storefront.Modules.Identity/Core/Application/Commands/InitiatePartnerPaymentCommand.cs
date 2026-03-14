using System.Globalization;
using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Storefront.Modules.Identity.Core.Domain.Entities;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record InitiatePartnerPaymentCommand(
    string PartnerUserId,
    decimal Amount,
    string ClientIp
) : IRequest<Result<InitiatePartnerPaymentDto>>;

public record InitiatePartnerPaymentDto(string Token, string CheckoutFormContent);

public class InitiatePartnerPaymentCommandHandler
    : IRequestHandler<InitiatePartnerPaymentCommand, Result<InitiatePartnerPaymentDto>>
{
    private readonly IdentityDbContext _context;
    private readonly IConfiguration _config;

    public InitiatePartnerPaymentCommandHandler(IdentityDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<Result<InitiatePartnerPaymentDto>> Handle(
        InitiatePartnerPaymentCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            return Error.Validation("Payment.InvalidAmount", "Ödeme tutarı sıfırdan büyük olmalıdır.");

        var user = await _context.PartnerUsers
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == request.PartnerUserId, cancellationToken);

        if (user is null)
            return Error.NotFound("Partner.NotFound", "Partner kullanıcısı bulunamadı.");

        var company = user.Company;

        var options = BuildOptions();
        var conversationId = Guid.NewGuid().ToString("N")[..20];
        var amountStr = request.Amount.ToString("F2", CultureInfo.InvariantCulture);
        var callbackUrl = _config["Iyzico:CallbackUrl"]
            ?? "http://localhost:8080/api/identity/partners/payments/callback";

        var buyer = new Buyer
        {
            Id = user.Id,
            Name = user.FirstName,
            Surname = user.LastName,
            GsmNumber = user.Phone ?? company.Phone,
            Email = user.Email,
            IdentityNumber = "11111111111",
            LastLoginDate = (user.LastLoginAt ?? user.CreatedAt).ToString("yyyy-MM-dd HH:mm:ss"),
            RegistrationDate = user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            RegistrationAddress = company.Address,
            Ip = request.ClientIp,
            City = company.City,
            Country = company.Country,
            ZipCode = company.PostalCode,
        };

        var address = new Address
        {
            ContactName = company.CompanyName,
            City = company.City,
            Country = company.Country,
            Description = company.Address,
            ZipCode = company.PostalCode,
        };

        var basketItem = new BasketItem
        {
            Id = $"payment-{conversationId}",
            Name = "Cari Hesap Ödemesi",
            Category1 = "Hesap",
            ItemType = BasketItemType.VIRTUAL.ToString(),
            Price = amountStr,
        };

        var initRequest = new CreateCheckoutFormInitializeRequest
        {
            Locale = Locale.TR.ToString(),
            ConversationId = conversationId,
            Price = amountStr,
            PaidPrice = amountStr,
            Currency = Currency.TRY.ToString(),
            BasketId = company.Id[..Math.Min(company.Id.Length, 36)],
            PaymentGroup = PaymentGroup.PRODUCT.ToString(),
            CallbackUrl = callbackUrl,
            Buyer = buyer,
            ShippingAddress = address,
            BillingAddress = address,
            BasketItems = [basketItem],
        };

        var formResult = CheckoutFormInitialize.Create(initRequest, options);

        if (formResult.Status != "success")
            return Error.Validation("Payment.IyzicoError",
                formResult.ErrorMessage ?? "Ödeme formu oluşturulamadı.");

        var payment = new PartnerPayment
        {
            PartnerCompanyId = company.Id,
            PartnerUserId = user.Id,
            IyzicoToken = formResult.Token,
            ConversationId = conversationId,
            Amount = request.Amount,
            Status = "Pending",
            CheckoutFormContent = formResult.CheckoutFormContent,
        };

        _context.PartnerPayments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<InitiatePartnerPaymentDto>.Success(
            new InitiatePartnerPaymentDto(formResult.Token, formResult.CheckoutFormContent));
    }

    private Options BuildOptions() => new()
    {
        ApiKey = _config["Iyzico:ApiKey"] ?? string.Empty,
        SecretKey = _config["Iyzico:SecretKey"] ?? string.Empty,
        BaseUrl = _config["Iyzico:BaseUrl"] ?? "https://sandbox-api.iyzipay.com",
    };
}
