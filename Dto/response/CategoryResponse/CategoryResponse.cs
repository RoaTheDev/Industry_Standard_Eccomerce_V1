namespace Ecommerce_site.Dto.response.CategoryResponse;

public class CategoryResponse
{
    public required string CategoryName { get; set; }
    public required string Description { get; set; }
    public required bool IsActive { get; set; }
}