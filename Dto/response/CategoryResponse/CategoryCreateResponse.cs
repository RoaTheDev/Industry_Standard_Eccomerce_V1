namespace Ecommerce_site.Dto.response.CategoryResponse;

public class CategoryCreateResponse
{
    public required long CategoryId { get; set; }
    public required string CategoryName { get; set; }
    public required string Description { get; set; }
    public required long CreatedBy { get; set; }
    public required bool IsActive { get; set; }
}