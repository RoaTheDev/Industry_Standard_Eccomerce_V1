namespace Ecommerce_site.Dto.response.ProductResponse;

public class ProductStatusResponse
{
    public required long ProductId { get; set; }
    public required bool IsAvailable { get; set; }
}