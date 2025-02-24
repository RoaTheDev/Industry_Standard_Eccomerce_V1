using Ecommerce_site.Dto.Request.ProductRequest;
using FluentValidation;

namespace Ecommerce_site.Validation.ProductValidation;

public class ProductRequestValidator<T> : AbstractValidator<T> where T : ProductRequest
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Price)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("The price cannot not be empty")
            .GreaterThan(0).WithMessage("The price must be greater than 0")
            .PrecisionScale(10, 2, true).WithMessage("Price must have at most 8 digits in total and 2 decimals.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("The description cannot be empty")
            .MaximumLength(255).WithMessage("Description cannot exceed 500 characters.")
            .When(p => !string.IsNullOrEmpty(p.Description));

        RuleFor(x => x.Discount)
            .InclusiveBetween(0, 100).WithMessage("Discount must be between 0 and 100.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0).WithMessage("The quantity can't be a negative value.");

        RuleFor(x => x.ProductName)
            .Matches(@"^[a-zA-Z\d]{3,}$").WithMessage("The product name can only contain character and number");
    }
}