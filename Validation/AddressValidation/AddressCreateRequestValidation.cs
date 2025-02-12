using Ecommerce_site.Dto.Request.AddressRequest;
using FluentValidation;

namespace Ecommerce_site.Validation.AddressValidation;

public class AddressCreateRequestValidation : AbstractValidator<AddressCreationRequest>
{
    public AddressCreateRequestValidation()
    {
        RuleFor(x => x.PostalCode).Matches(@"^\d{4,6}$").WithMessage("Invalid Postal Code");

        RuleFor(x => x.Country)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Country name cannot be empty")
            .Matches(@"^[a-zA-Z\d]{2,}$").WithMessage("Country can't contain any special character")
            .Length(2, 100).WithMessage("Country can only be between 2 to 100");

        RuleFor(x => x.State)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("State cannot be empty")
            .Matches(@"^[a-zA-Z\d]{2,}$").WithMessage("State can't contain any special character")
            .Length(2, 100).WithMessage("State can only be between 2 to 100");

        RuleFor(x => x.FirstAddressLine)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("First address line cannot be empty")
            .Matches(@"^[a-zA-Z\d\s]{5,}$").WithMessage("First Address can't contain any special character")
            .Length(5, 100).WithMessage("First address can only be between 5 to 100");

        RuleFor(x => x.SecondAddressLine)
            .Cascade(CascadeMode.Stop)
            .Matches(@"^[a-zA-Z\d\s]{0,}$").WithMessage("Second address line can't contain any special character")
            .Length(0, 100).WithMessage("Second address can only be between 0 to 100");
    }
}