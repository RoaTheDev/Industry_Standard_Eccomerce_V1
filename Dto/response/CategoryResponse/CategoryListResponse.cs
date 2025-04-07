namespace Ecommerce_site.Dto.response.CategoryResponse;

public class CategoryListResponse : CategoryResponse
{
    public required long CategoryId { get; set; }
    public required bool IsActive { get; set; }
}

public class PaginatedCategoryResponse
{
    public required List<CategoryListResponse> Categories { get; set; }
    public required int Cursor { get; set; }
    public required int PageSize { get; set; }
}