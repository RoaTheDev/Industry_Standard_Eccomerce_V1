namespace Ecommerce_site.Dto.response.ProductResponse;

public class ProductImageAddResponse
{
    public required long ProductId { get; set; }
    public required IEnumerable<string> ImageUrls { get; set; }
}