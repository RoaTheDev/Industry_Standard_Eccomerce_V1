namespace Ecommerce_site.Dto.Request.AddressRequest;

public class AddressUpdateRequest
{
    public long AddressId { get; set; }

    public string FirstAddressLine { get; set; } = string.Empty;

    public string? SecondAddressLine { get; set; }

    public string City { get; set; } = string.Empty;

    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string Country { get; set; } = string.Empty;
}