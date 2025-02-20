using Ecommerce_site.Dto.Request.CustomerRequest;
using FluentValidation;

namespace Ecommerce_site.Validation.CustomerValidation;

public class CustomerRegisterRequestUapValidator : AbstractValidator<CustomerRegisterRequestUap>
{
    public CustomerRegisterRequestUapValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email must not be empty")
            .EmailAddress().WithMessage("Must be a valid email.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password must not be empty")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$")
            .WithMessage("Password must have at least one uppercase, one lowercase, and a number.")
            .Length(8, 50).WithMessage("Password must be between 8 and 50 characters long.");

        RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("The password must match.");

        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Your firstname cannot be empty")
            .Must(firstName => !firstName.Contains(' ')).WithMessage("Firstname can't contain spaces")
            .Matches(@"^[a-zA-Z]{3,}$").WithMessage("Firstname can't contain any special character")
            .Length(3, 50).WithMessage("Firstname can only be between 4 to 50");

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Your  lastname cannot be empty")
            .Must(lastName => !lastName.Contains(' ')).WithMessage("Lastname can't contain spaces")
            .Matches(@"^[a-zA-Z]{3,}$").WithMessage("Lastname can't contain any special character")
            .Length(3, 50).WithMessage("Lastname can only be between 4 to 50");

        RuleFor(x => x.MiddleName)
            .Cascade(CascadeMode.Stop)
            .Matches(@"^[a-zA-Z]{0,}$").WithMessage("Middle name can't contain any special character")
            .Length(0, 50).WithMessage("Middle name can only be between 8 to 50");

        RuleFor(x => x.Dob)
            .NotEmpty().WithMessage("Date of birth is required.")
            .Must(BeAtLeast15YearsOld).WithMessage("You must be at least 15 years old.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+[1-9]\d{1,14}$")
            .WithMessage("Phone number must be in international format (e.g., +1234567890).");
    }

    private bool BeAtLeast15YearsOld(DateOnly dob)
    {
        // Get today's date.
        var today = DateTime.Today;

        int age = today.Year - dob.Year;

        // This is used to check whether the birthday has already occurred this year.
        DateOnly birthdayThisYear = new DateOnly(today.Year, dob.Month, dob.Day);

        // If today's date is before the birthday, the person hasn't turned a year older yet.
        if (DateOnly.FromDateTime(today) < birthdayThisYear)
        {
            age--;
        }

        return age >= 15;
    }
}