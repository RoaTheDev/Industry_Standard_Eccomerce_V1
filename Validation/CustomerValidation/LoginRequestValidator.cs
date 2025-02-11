using Ecommerce_site.Dto.Request.CustomerRequest;
using FluentValidation;

namespace Ecommerce_site.Validation.CustomerValidation;

public class LoginRequestValidator : AbstractValidator<LoginRequestUap>
{
    public LoginRequestValidator()
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
    }
}