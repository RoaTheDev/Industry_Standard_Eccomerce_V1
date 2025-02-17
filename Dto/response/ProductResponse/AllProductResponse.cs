namespace Ecommerce_site.Dto.response.ProductResponse;

public class AllProductResponse : ProductResponse
{
    public required string ImageUrl { get; set; }
    public required string CategoryName { get; set; }

}