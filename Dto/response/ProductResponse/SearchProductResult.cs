namespace Ecommerce_site.Dto.response.ProductResponse;

public class SearchProductResult
{
    public long ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? Description { get; set; }
    public int Discount { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime CreateAt { get; set; }
    public string? CategoryName { get; set; }
    public string? ImageUrls { get; set; }
    public string? Tags { get; set; }
}