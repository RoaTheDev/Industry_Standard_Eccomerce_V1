namespace Ecommerce_site.Dto.Request.CustomerRequest;

public class CustomerUpdateRequest
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? Gender { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public DateOnly? Dob { get; set; }
}