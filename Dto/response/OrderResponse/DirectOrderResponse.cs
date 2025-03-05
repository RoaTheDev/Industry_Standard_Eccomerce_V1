namespace Ecommerce_site.Dto.response.OrderResponse;

public class DirectOrderResponse
{
    public required long OrderId { get; set; }
    public required long CustomerId { get; set; }
    public required string OrderNumber { get; set; }
    public required DateTime OrderDate { get; set; }
    public required long BillingAddressId { get; set; }
    public required long ShippingAddressId { get; set; }
    public required decimal TotalBaseAmount { get; set; }
    public required decimal TotalDiscountAmount { get; set; }
    public required decimal ShippingCost { get; set; }
    public required decimal TotalAmount { get; set; }
    public required string OrderStatus { get; set; }
    public required OrderItemResponse OrderItem { get; set; }
}