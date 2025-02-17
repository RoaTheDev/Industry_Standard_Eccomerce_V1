namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductImageChangeRequest
{
    public required long ProductId { get; set; }
    public required long ImageId { get; set; }
    public required string ImageUrl { get; set; }
}