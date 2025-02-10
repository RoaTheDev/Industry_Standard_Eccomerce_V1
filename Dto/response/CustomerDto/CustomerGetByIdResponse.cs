namespace Ecommerce_site.Dto.response.CustomerDto;

public class CustomerGetByIdResponse
{
    public required long CustomerId { get; set; }
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public required string LastName { get; set; }

    public required string Gender { get; set; }

    public required string Email { get; set; }
    public required DateOnly Dob { get; set; }
    public required string PhoneNumber { get; set; }
}