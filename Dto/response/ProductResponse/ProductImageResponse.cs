namespace Ecommerce_site.Dto.response.ProductResponse;

public class ProductImageResponse
{
    public required long ProductId { get; set; }
    public required IEnumerable<ImageResponse> Images { get; set; }
}

public class ImageResponse
{
    public required long ImageId { get; set; }
    public required string ImageUrl { get; set; }
}