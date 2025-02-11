using Ecommerce_site.Dto.Request.CustomerDto;
using FluentValidation;

namespace Ecommerce_site.Validation;

public class PasswordChangeRequestValidator : AbstractValidator<PasswordChangeRequest>
{
    public PasswordChangeRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password must not be empty")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$")
            .WithMessage("Password must have at least one uppercase, one lowercase, and a number.")
            .Length(8, 50).WithMessage("Password must be between 8 and 50 characters long.");
        
        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password must not be empty")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$")
            .WithMessage("Password must have at least one uppercase, one lowercase, and a number.")
            .Length(8, 50).WithMessage("Password must be between 8 and 50 characters long.");

        RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessage("The password must match.");
    }
}