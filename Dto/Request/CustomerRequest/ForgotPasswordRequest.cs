using System.ComponentModel.DataAnnotations;

namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public required string Email { get; set; }
}