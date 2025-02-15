namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductCreateRequest : ProductRequest
{
    public required IEnumerable<string> ImageUrl { get; set; }
}