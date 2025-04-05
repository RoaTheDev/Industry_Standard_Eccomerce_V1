using Ecommerce_site.config.Rule;
using Newtonsoft.Json;

namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class CustomerRegisterRequestUap
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public DateOnly Dob { get; set; } = default;

    public required string PhoneNumber { get; set; }
}