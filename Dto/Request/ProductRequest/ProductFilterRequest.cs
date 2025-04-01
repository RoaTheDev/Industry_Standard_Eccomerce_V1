using Ecommerce_site.Model.Enum;

namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductFilterRequest
{
    public long? CategoryId { get; set; }
    public List<long>? TagIds { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public bool IsLatest { get; set; }
    public SortByEnum? SortBy { get; set; }

    public string? SearchQuery { get; set; }
}