using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Storefront.Modules.Identity.Core.Application.Commands;
using Storefront.Modules.Identity.Infrastructure.Persistence;
using System.Security.Claims;

namespace Storefront.Modules.Identity.API.Controllers;

[ApiController]
[Route("api/identity/partners/payments")]
public class PartnerPaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IdentityDbContext _context;

    public PartnerPaymentsController(IMediator mediator, IdentityDbContext context)
    {
        _mediator = mediator;
        _context = context;
    }

    /// <summary>
    /// Initiate an iyzico payment for the partner's account balance.
    /// Returns the iyzico checkout form content to be rendered in the browser.
    /// </summary>
    [HttpPost("initialize")]
    [Authorize(Roles = "Partner")]
    public async Task<IActionResult> Initialize([FromBody] InitiatePaymentRequest request, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User ID not found");

        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        var result = await _mediator.Send(
            new InitiatePartnerPaymentCommand(userId, request.Amount, clientIp), ct);

        return result.IsSuccess
            ? Ok(new { token = result.Value.Token, checkoutFormContent = result.Value.CheckoutFormContent })
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Serves the iyzico payment form HTML page by token.
    /// Used by mobile to open the form in a browser without re-authenticating.
    /// </summary>
    [HttpGet("form/{token}")]
    public async Task<IActionResult> GetForm(string token, CancellationToken ct)
    {
        var payment = await _context.PartnerPayments
            .FirstOrDefaultAsync(p => p.IyzicoToken == token && p.Status == "Pending", ct);

        if (payment is null)
            return NotFound("Ödeme formu bulunamadı veya süresi doldu.");

        var html = $$"""
            <!DOCTYPE html>
            <html lang="tr">
            <head>
              <meta charset="UTF-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1.0" />
              <title>Ödeme</title>
              <style>
                body { margin: 0; display: flex; align-items: center; justify-content: center; min-height: 100vh; background: #f9fafb; font-family: sans-serif; }
                .loading { text-align: center; color: #6b7280; }
              </style>
            </head>
            <body>
              <div class="loading" id="loading">Ödeme formu yükleniyor...</div>
              {{payment.CheckoutFormContent}}
              <script>document.getElementById('loading').style.display='none';</script>
            </body>
            </html>
            """;

        return Content(html, "text/html");
    }

    /// <summary>
    /// iyzico posts here after payment is completed (success or failure).
    /// Processes the result and redirects the browser to the frontend result page.
    /// </summary>
    [HttpPost("callback")]
    public async Task<IActionResult> Callback([FromForm] IyzicoCallbackForm form, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new ProcessPartnerPaymentCallbackCommand(form.Token ?? string.Empty, form.Status), ct);

        var redirectUrl = result.IsSuccess
            ? result.Value
            : "http://localhost:3000/partner/payments/result?status=failed";

        return Redirect(redirectUrl);
    }
}

public record InitiatePaymentRequest(decimal Amount);

public record IyzicoCallbackForm
{
    [Microsoft.AspNetCore.Mvc.ModelBinder]
    public string? Token { get; init; }
    [Microsoft.AspNetCore.Mvc.ModelBinder]
    public string? Status { get; init; }
}
