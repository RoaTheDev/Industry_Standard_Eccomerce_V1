namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductUpdateRequest : ProductRequest
{
    public required long UpdatedBy { get; set; }
}