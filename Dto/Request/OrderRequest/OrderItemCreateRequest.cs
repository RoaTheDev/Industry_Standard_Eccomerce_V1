namespace Ecommerce_site.Dto.Request.OrderRequest;

public class OrderItemCreateRequest
{
    public required long ProductId { get; set; }
    public required long Quantity { get; set; }
}