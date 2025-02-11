namespace Ecommerce_site.Dto.Request.AddressRequest;

public class AddressDeleteRequest
{
    public required long AddressId { get; set; }
    public required long CustomerId { get; set; }
}