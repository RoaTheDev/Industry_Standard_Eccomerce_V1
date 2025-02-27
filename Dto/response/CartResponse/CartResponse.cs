namespace Ecommerce_site.Dto.response.CartResponse;

public class CartResponse
{
    public required long CartId { get; set; }
    public required long CustomerId { get; set; }
    public required bool IsCheckedOut { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required decimal TotalBasePrice { get; set; }
    public required decimal TotalDiscount { get; set; }
    public required decimal TotalAmount { get; set; }
    public required List<CartItemResponse> CartItems { get; set; }
}