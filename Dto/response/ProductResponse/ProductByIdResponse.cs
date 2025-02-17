namespace Ecommerce_site.Dto.response.ProductResponse;

public class ProductByIdResponse : ProductResponse
{
    public required string CategoryName { get; set; }
    public required IEnumerable<string> ImageUrls { get; set; }
    public required IEnumerable<string> Tags { get; set; }
}