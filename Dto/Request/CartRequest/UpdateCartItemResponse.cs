namespace Ecommerce_site.Dto.Request.CartRequest;

public class UpdateCartItemResponse
{
    public required long CartItemId { get; set; }
    public required long Quantity { get; set; }
}