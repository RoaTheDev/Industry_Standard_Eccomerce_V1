namespace Ecommerce_site.Dto.Request.AddressRequest;

public class AddressCreationRequest
{
    public required long CustomerId { get; set; }
    public required string FirstAddressLine { get; set; }
    public string? SecondAddressLine { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
}