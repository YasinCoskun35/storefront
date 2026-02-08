using FluentValidation;

namespace Storefront.Modules.Identity.Core.Application.Commands;

public class PartnerLoginCommandValidator : AbstractValidator<PartnerLoginCommand>
{
    public PartnerLoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
