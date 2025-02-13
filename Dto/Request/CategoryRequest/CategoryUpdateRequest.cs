namespace Ecommerce_site.Dto.Request.CategoryRequest;

public class CategoryUpdateRequest
{
    public required long CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public required string Description { get; set; }
    public required long UpdatedBy { get; set; }
}