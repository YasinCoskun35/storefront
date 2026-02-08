using MediatR;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public record PartnerLoginCommand(
    string Email,
    string Password
) : IRequest<Result<PartnerLoginResponse>>;

public record PartnerLoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    PartnerUserInfo User
);

public record PartnerUserInfo(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    PartnerCompanyInfo Company
);

public record PartnerCompanyInfo(
    string Id,
    string Name,
    string Status
);
