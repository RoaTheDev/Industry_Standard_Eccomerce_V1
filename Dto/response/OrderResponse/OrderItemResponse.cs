namespace Ecommerce_site.Dto.response.OrderResponse;

public class OrderItemResponse
{
    public required string ProductName { get; set; }
    public required long OrderItemId { get; set; }
    public required long ProductId { get; set; }
    public required decimal UnitPrice { get; set; }
    public required decimal Discount { get; set; }
    public required decimal TotalPrice { get; set; }
    public required long Quantity { get; set; }
}