namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductTagRemoveRequest
{
    public required IList<long> TagIds { get; set; }
}