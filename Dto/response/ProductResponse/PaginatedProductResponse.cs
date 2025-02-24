namespace Ecommerce_site.Dto.response.ProductResponse;

public class PaginatedProductResponse
{
    public required List<ProductResponse> Products { get; set; }
    public long? NextCursor { get; set; }
    public required int PageSize { get; set; }
}