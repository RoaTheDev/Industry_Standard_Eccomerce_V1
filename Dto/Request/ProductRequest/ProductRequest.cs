namespace Ecommerce_site.Dto.Request.ProductRequest;

public class ProductRequest
{
    public required string ProductName { get; set; }
    public required string Description { get; set; }
    public required decimal Price { get; set; }
    public bool? IsAvailable { get; set; }
    public required int Quantity { get; set; }
    public required int Discount { get; set; }
    public required long CategoryId { get; set; }
}