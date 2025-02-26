
namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductCreateRequest : ProductRequest
{
    
    public required long CreateBy { get; set; }
    public bool? IsAvailable { get; set; }
    
    public required IList<long> TagIds { get; set; }
}