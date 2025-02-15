namespace Ecommerce_site.Dto.response.ProductResponse;

public class ProductImageChangeResponse
{
    public required long ProductId { get; set; }
    public required long ImageId { get; set; }
    public required string ImageUrl { get; set; }
}