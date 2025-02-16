namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductUpdateRequest : ProductRequest
{
    public required long ProductId { get; set; }
    public required DateTime UpdatedAt { get; set; }
}