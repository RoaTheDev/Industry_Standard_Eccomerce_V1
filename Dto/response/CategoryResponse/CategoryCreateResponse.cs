namespace Ecommerce_site.Dto.response.CategoryResponse;

public class CategoryCreateResponse : CategoryResponse
{
    public required long CategoryId { get; set; }
    public required long CreatedBy { get; set; }
    public required bool IsActive { get; set; }
}