using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record ApprovePartnerCommand(
    string PartnerCompanyId,
    string AdminUserId,
    string? ApprovalNotes
) : IRequest<Result>;
