namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductImageAddRequest
{
    public required long ProductId { get; set; }
    public required IEnumerable<string> ImageUrls { get; set; }
}