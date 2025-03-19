using System.ComponentModel.DataAnnotations;

namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class ForgotPasswordRequest
{
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public string? Email { get; set; }
}