namespace Ecommerce_site.Dto.response.ProductResponse;

public class PaginatedProductResponse
{
    public required List<PaginatedProduct> Products { get; set; }
    public long? NextCursor { get; set; }
    public required int PageSize { get; set; }
    public AppliedProductFilters? AppliedFilters { get; set; } 
}

public  class PaginatedProduct : ProductResponse
{
    public required DateTime CreateAt { get; set; }
    public required string CategoryName { get; set; }
    public required string ImageUrls { get; set; }
    public required IEnumerable<string> Tags { get; set; }
}

public class AppliedProductFilters
{
    public long? CategoryId { get; set; }
    public List<long>? TagIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public string? SortBy { get; set; } // "price", "date", "name"
    public string? SortOrder { get; set; } // "asc", "desc"
}