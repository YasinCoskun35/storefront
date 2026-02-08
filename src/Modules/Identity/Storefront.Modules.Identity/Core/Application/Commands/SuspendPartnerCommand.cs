using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record SuspendPartnerCommand(
    string PartnerCompanyId,
    string AdminUserId,
    string? Reason
) : IRequest<Result>;
