namespace Ecommerce_site.Dto.response.CustomerDto;

public class CustomerUpdateResponse
{
    public required string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Gender { get; set; }
    public required string LastName { get; set; }
    public required string PhoneNUmber { get; set; }
    public required string Email { get; set; }
    public required string Dob { get; set; }
}