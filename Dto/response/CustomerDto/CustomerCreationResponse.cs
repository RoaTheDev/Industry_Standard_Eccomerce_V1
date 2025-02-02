namespace Ecommerce_site.Dto.response.CustomerDto;

public class CustomerCreationResponse
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required DateOnly Dob { get; set; }
    public required string PhoneNumber { get; set; }
    public required TokenResponse Token { get; set; }
}