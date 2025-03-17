using System.ComponentModel.DataAnnotations;

namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class ResetPasswordRequest
{
    [MinLength(8, ErrorMessage = "The minimum password is 8 characters")]
    public required string Password { get; set; }

    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public required string ConfirmPassword { get; set; }
}