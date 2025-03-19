namespace Ecommerce_site.Dto.Request.CategoryRequest;

public class CategoryUpdateRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public required long UpdatedBy { get; set; }
}