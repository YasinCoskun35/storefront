using FluentValidation;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.PartnerUserId)
            .NotEmpty().WithMessage("Partner user ID is required");

        RuleFor(x => x.PartnerCompanyId)
            .NotEmpty().WithMessage("Partner company ID is required");

        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().WithMessage("Delivery address is required")
            .MaximumLength(500);

        RuleFor(x => x.DeliveryCity)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100);

        RuleFor(x => x.DeliveryState)
            .NotEmpty().WithMessage("State/Province is required")
            .MaximumLength(100);

        RuleFor(x => x.DeliveryPostalCode)
            .NotEmpty().WithMessage("Postal code is required")
            .MaximumLength(20);

        RuleFor(x => x.DeliveryCountry)
            .NotEmpty().WithMessage("Country is required")
            .MaximumLength(100);

        RuleFor(x => x.DeliveryNotes)
            .MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.DeliveryNotes));

        RuleFor(x => x.Notes)
            .MaximumLength(5000).When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
