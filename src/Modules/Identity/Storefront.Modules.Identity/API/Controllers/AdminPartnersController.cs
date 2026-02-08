using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Storefront.Modules.Identity.Core.Application.Commands;
using Storefront.Modules.Identity.Core.Application.Queries;

namespace Storefront.Modules.Identity.API.Controllers;

[ApiController]
[Route("api/admin/partners")]
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
