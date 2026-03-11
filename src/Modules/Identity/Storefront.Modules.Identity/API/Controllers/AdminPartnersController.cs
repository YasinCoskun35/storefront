using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Identity.Core.Application.Commands;
using Storefront.Modules.Identity.Core.Application.Queries;

namespace Storefront.Modules.Identity.API.Controllers;

[ApiController]
[Route("api/identity/admin/partners")]
[Authorize(Roles = "Admin")]
public class AdminPartnersController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminPartnersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all partner companies (admin only)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPartners(
        [FromQuery] string? searchTerm,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = new GetPartnerCompaniesQuery(searchTerm, status, pageNumber, pageSize);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Create new partner company (admin only)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePartner([FromBody] CreatePartnerRequest request)
    {
        // Get admin user ID from claims
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Admin user ID not found");

        var command = new CreatePartnerCommand(
            request.CompanyName,
            request.TaxId,
            request.Email,
            request.Phone,
            request.Address,
            request.City,
            request.State,
            request.PostalCode,
            request.Country,
            request.Industry,
            request.Website,
            request.EmployeeCount,
            request.AnnualRevenue,
            new AdminUserInfo(
                request.AdminUser.FirstName,
                request.AdminUser.LastName,
                request.AdminUser.Email,
                request.AdminUser.Password
            ),
            adminUserId
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Created($"/api/admin/partners/{result.Value}", new { id = result.Value })
            : result.Error.Code == "Partner.TaxIdAlreadyExists" || result.Error.Code == "Partner.EmailAlreadyExists"
                ? Conflict(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Get partner company details (admin only)
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPartnerDetails(string id)
    {
        var query = new GetPartnerCompanyDetailsQuery(id);
        var result = await _mediator.Send(query);

        return result.IsSuccess
            ? Ok(result.Value)
            : result.Error.Code == "Partner.NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Approve partner registration (admin only)
    /// </summary>
    [HttpPut("{id}/approve")]
    public async Task<IActionResult> ApprovePartner(string id, [FromBody] ApprovePartnerRequest request)
    {
        // Get admin user ID from claims
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Admin user ID not found");

        var command = new ApprovePartnerCommand(id, adminUserId, request.ApprovalNotes);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Partner approved successfully" })
            : result.Error.Code == "Partner.NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Add a user to an existing partner company (admin only)
    /// </summary>
    [HttpPost("{id}/users")]
    public async Task<IActionResult> AddPartnerUser(string id, [FromBody] AddPartnerUserRequest request)
    {
        var command = new AddPartnerUserCommand(
            id,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Role,
            request.Scopes
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Created($"/api/identity/admin/partners/{id}/users/{result.Value}", new { id = result.Value })
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Conflict" => Conflict(new { error = result.Error.Code, message = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    /// <summary>
    /// Suspend partner account (admin only)
    /// </summary>
    [HttpPut("{id}/suspend")]
    public async Task<IActionResult> SuspendPartner(string id, [FromBody] SuspendPartnerRequest request)
    {
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Admin user ID not found");

        var command = new SuspendPartnerCommand(id, adminUserId, request.Reason);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Partner suspended successfully" })
            : result.Error.Code == "Partner.NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Update a partner user's details (admin only)
    /// </summary>
    [HttpPut("users/{userId}")]
    public async Task<IActionResult> UpdatePartnerUser(string userId, [FromBody] UpdatePartnerUserRequest request)
    {
        var command = new UpdatePartnerUserCommand(
            userId,
            request.FirstName,
            request.LastName,
            request.Phone,
            request.Role,
            request.IsActive,
            request.Scopes
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "User updated successfully" })
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    /// <summary>
    /// Reset a partner user's password (admin only)
    /// </summary>
    [HttpPut("users/{userId}/reset-password")]
    public async Task<IActionResult> ResetPartnerUserPassword(string userId, [FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPartnerUserPasswordCommand(userId, request.NewPassword);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Password reset successfully" })
            : result.Error.Type == "NotFound"
                ? NotFound(new { error = result.Error.Code, message = result.Error.Message })
                : BadRequest(new { error = result.Error.Code, message = result.Error.Message });
    }

    /// <summary>
    /// Update partner pricing policy (discount rate) (admin only)
    /// </summary>
    [HttpPut("{id}/pricing")]
    public async Task<IActionResult> UpdatePartnerPricing(string id, [FromBody] UpdatePartnerPricingRequest request)
    {
        var command = new UpdatePartnerPricingCommand(id, request.DiscountRate);
        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Ok(new { message = "Pricing updated successfully" })
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Code, message = result.Error.Message })
            };
    }

    /// <summary>
    /// Record an account transaction (debit/credit) for a partner (admin only)
    /// </summary>
    [HttpPost("{id}/account/transactions")]
    public async Task<IActionResult> RecordAccountTransaction(string id, [FromBody] RecordTransactionRequest request)
    {
        var adminUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Admin user ID not found");

        if (!System.Enum.TryParse<Storefront.Modules.Identity.Core.Domain.Enums.TransactionType>(request.Type, true, out var transactionType))
        {
            return BadRequest(new { error = "Transaction.InvalidType", message = "Invalid transaction type" });
        }

        Storefront.Modules.Identity.Core.Domain.Enums.PaymentMethod? paymentMethod = null;
        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            if (!System.Enum.TryParse<Storefront.Modules.Identity.Core.Domain.Enums.PaymentMethod>(request.PaymentMethod, true, out var pm))
            {
                return BadRequest(new { error = "Transaction.InvalidPaymentMethod", message = "Invalid payment method" });
            }
            paymentMethod = pm;
        }

        var command = new RecordAccountTransactionCommand(
            id,
            transactionType,
            request.Amount,
            paymentMethod,
            request.OrderReference,
            request.Notes,
            adminUserId
        );

        var result = await _mediator.Send(command);

        return result.IsSuccess
            ? Created($"/api/identity/admin/partners/{id}/account/transactions/{result.Value}", new { id = result.Value })
            : result.Error.Type switch
            {
                "NotFound" => NotFound(new { error = result.Error.Code, message = result.Error.Message }),
                "Validation" => BadRequest(new { error = result.Error.Code, message = result.Error.Message }),
                _ => BadRequest(new { error = result.Error.Code, message = result.Error.Message })
            };
    }
}

public record ApprovePartnerRequest(string? ApprovalNotes);
public record SuspendPartnerRequest(string? Reason);
public record CreatePartnerRequest(
    string CompanyName,
    string TaxId,
    string Email,
    string Phone,
    string Address,
    string City,
    string State,
    string PostalCode,
    string Country,
    string? Industry,
    string? Website,
    int? EmployeeCount,
    decimal? AnnualRevenue,
    AdminUserDto AdminUser
);

public record AdminUserDto(
    string FirstName,
    string LastName,
    string Email,
    string Password
);

public record AddPartnerUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string Role,
    List<string>? Scopes = null
);

public record UpdatePartnerUserRequest(
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    bool IsActive,
    List<string>? Scopes = null
);

public record ResetPasswordRequest(string NewPassword);
public record UpdatePartnerPricingRequest(decimal DiscountRate);
public record RecordTransactionRequest(
    string Type,
    decimal Amount,
    string? PaymentMethod,
    string? OrderReference,
    string? Notes
);
