namespace Ecommerce_site.Dto.response.ProductResponse;

public class PaginatedProductResponse
{
    public required List<PaginatedProduct> Products { get; set; }
    public long? NextCursor { get; set; }
    public required int PageSize { get; set; }
}

public  class PaginatedProduct : ProductResponse
{
    public required string CategoryName { get; set; }
    public required string ImageUrls { get; set; }
    public required IEnumerable<string> Tags { get; set; }
}