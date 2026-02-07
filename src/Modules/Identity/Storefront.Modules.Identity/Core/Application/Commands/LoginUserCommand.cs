using MediatR;
using Storefront.Modules.Identity.Core.Application.DTOs;
using Storefront.SharedKernel;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public sealed record LoginUserCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResponse>>;

