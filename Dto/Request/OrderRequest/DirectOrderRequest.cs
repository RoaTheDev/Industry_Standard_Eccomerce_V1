namespace Ecommerce_site.Dto.Request.OrderRequest;

public class DirectOrderRequest
{
    public required long BillingAddressId { get; set; }
    public required long ShippingAddressId { get; set; }
    public required OrderItemCreateRequest OrderItem { get; set; }
}