
namespace Ecommerce_site.Dto.Request.CategoryRequest;

public class CategoryCreateRequest
{
    public required string CategoryName { get; set; }

    public required string Description { get; set; }

    public required long CreateBy { get; set; }
}