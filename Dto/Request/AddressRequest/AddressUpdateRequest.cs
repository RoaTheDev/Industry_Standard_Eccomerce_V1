namespace Ecommerce_site.Dto.Request.AddressRequest;

public class AddressUpdateRequest
{
    public required long AddressId { get; set; }
    
    public required string FirstAddressLine { get; set; }

    public string? SecondAddressLine { get; set; }

    public required string City { get; set; }

    public required string State { get; set; }

    public required string PostalCode { get; set; }

    public required string Country { get; set; }
}