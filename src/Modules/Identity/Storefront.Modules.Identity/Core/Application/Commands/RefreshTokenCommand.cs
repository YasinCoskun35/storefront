using MediatR;
using Storefront.Modules.Identity.Core.Application.DTOs;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed record RefreshTokenCommand(
    string RefreshToken
) : IRequest<Result<LoginResponse>>;

