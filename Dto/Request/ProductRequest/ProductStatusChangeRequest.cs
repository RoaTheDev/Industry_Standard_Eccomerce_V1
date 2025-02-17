namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductStatusChangeRequest
{
    public required long ProductId { get; set; }
    public required bool IsAvailable { get; set; }
}