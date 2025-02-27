namespace Ecommerce_site.Dto.response.CartResponse;

public class CartItemResponse
{
    public required long CartItemsId { get; set; }
    public required long ProductId { get; set; }
    public required string ProductName { get; set; }
    public required long Quantity { get; set; }
    public required decimal UnitPrice { get; set; }
    public required decimal Discount { get; set; }
    public required decimal TotalPrice { get; set; }
}