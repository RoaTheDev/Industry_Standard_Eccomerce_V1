namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductTagRemoveRequest
{
    public required long ProductId { get; set; }
    public required long TagId { get; set; }
}