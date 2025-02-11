namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class CustomerRegisterRequestUap
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public string? Gender { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public required DateOnly Dob { get; set; }
    public required string PhoneNumber { get; set; }
}