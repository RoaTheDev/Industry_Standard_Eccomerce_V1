namespace Ecommerce_site.Dto.response.ProductResponse;

public class ProductCreateResponse : ProductResponse
{
    public required long CreateBy { get; set; }
    public required DateTime CreateAt { get; set; }
    public required bool IsAvailable { get; set; }
}