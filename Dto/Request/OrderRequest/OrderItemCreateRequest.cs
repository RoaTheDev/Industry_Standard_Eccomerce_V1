using System.ComponentModel.DataAnnotations;

namespace Ecommerce_site.Dto.Request.OrderRequest;

public class OrderItemCreateRequest
{
    public required long ProductId { get; set; }

    [Range(1, 100, ErrorMessage = "The quantity must be between 1 to 100")]
    public required long Quantity { get; set; }
}