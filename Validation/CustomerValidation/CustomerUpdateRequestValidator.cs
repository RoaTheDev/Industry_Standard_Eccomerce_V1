using Ecommerce_site.Dto.Request.CustomerRequest;
using FluentValidation;

namespace Ecommerce_site.Validation.CustomerValidation;

public class CustomerUpdateRequestValidator : AbstractValidator<CustomerUpdateRequest>
{
    public CustomerUpdateRequestValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email must not be empty")
            .EmailAddress().WithMessage("Must be a valid email.");

        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Your firstname cannot be empty")
            .Must(firstName => firstName != null && !firstName.Contains(' '))
            .WithMessage("Firstname can't contain spaces")
            .Matches(@"^[a-zA-Z]{4,}$").WithMessage("Firstname can't contain any special character")
            .Length(4, 50).WithMessage("Firstname can only be between 4 to 50");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Your  lastname cannot be empty")
            .Must(lastName => lastName != null && !lastName.Contains(' ')).WithMessage("Lastname can't contain spaces")
            .Matches(@"^[a-zA-Z]{4,}$").WithMessage("Lastname can't contain any special character")
            .Length(4, 50).WithMessage("Lastname can only be between 4 to 50");

        RuleFor(x => x.MiddleName)
            .Cascade(CascadeMode.Stop)
            .Matches(@"^[a-zA-Z]{0,}$").WithMessage("Middle name can't contain any special character")
            .Length(0, 50).WithMessage("Middle name can only be between 0 to 50");

        RuleFor(x => x.Dob)
            .NotEmpty().WithMessage("Date of birth is required.").Must(dob =>
            {
                var today = DateTime.Today;
                if (dob != null)
                {
                    int age = today.Year - dob.Value.Year;

                    if (DateOnly.FromDateTime(today) < new DateOnly(today.Year, dob.Value.Month, dob.Value.Day))
                    {
                        age--;
                    }

                    return age >= 15;
                }

                return false;
            }).WithMessage("You must be at least 15 years old.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in international format (e.g., +1234567890).");
    }
}