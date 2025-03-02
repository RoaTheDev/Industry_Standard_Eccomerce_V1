namespace Ecommerce_site.Dto.Request.OrderRequest;

public class OrderCreateRequest
{
    public required long CustomerId { get; set; }
    public required long BillingAddressId { get; set; }
    public required long ShippingAddressId { get; set; }
    public required List<OrderItemCreateRequest> OrderItems { get; set; }
    
}