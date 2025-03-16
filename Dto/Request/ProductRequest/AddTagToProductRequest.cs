namespace Ecommerce_site.Dto.Request.ProductRequest;

public class AddTagToProductRequest
{
    public required IList<long> TagIds { get; set; }
}