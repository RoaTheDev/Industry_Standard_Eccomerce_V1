namespace Ecommerce_site.Dto.Request.CustomerDto;

public class CustomerRegisterRequestUap
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string ConfirmPassword { get; set; }
    public required DateOnly Dob { get; set; }
    public required string PhoneNumber { get; set; }
}