namespace Ecommerce_site.Dto.Request.CustomerDto;

public class CustomerUpdateRequest
{
    public required long CustomerId { get; set; }
    public  string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Gender { get; set; }
    public  string? LastName { get; set; }
    public  string? PhoneNUmber { get; set; }
    public  string? Email { get; set; }
    public  string? Dob { get; set; }
}