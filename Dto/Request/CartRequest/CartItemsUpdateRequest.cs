using System.ComponentModel.DataAnnotations;

namespace Ecommerce_site.Dto.Request.CartRequest;

public class CartItemsUpdateRequest
{
    public required long CartItemsId { get; set; }

    [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
    public required long Quantity { get; set; }
}