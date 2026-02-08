using FluentValidation;

namespace Storefront.Modules.Orders.Core.Application.Commands;

public class AddOrderCommentCommandValidator : AbstractValidator<AddOrderCommentCommand>
{
    public AddOrderCommentCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required")
            .MaximumLength(5000);

        RuleFor(x => x.AuthorId)
            .NotEmpty().WithMessage("Author ID is required");

        RuleFor(x => x.AuthorName)
            .NotEmpty().WithMessage("Author name is required")
            .MaximumLength(200);

        RuleFor(x => x.AuthorType)
            .NotEmpty().WithMessage("Author type is required")
            .Must(x => x == "Admin" || x == "Partner" || x == "System")
            .WithMessage("Author type must be Admin, Partner, or System");
    }
}
