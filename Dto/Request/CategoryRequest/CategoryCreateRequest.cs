
namespace Ecommerce_site.Dto.Request.CategoryRequest;

public class CategoryCreateRequest
{
    public string CategoryName { get; set; } = string.Empty;

    public  string Description { get; set; }= string.Empty;

    public required long CreateBy { get; set; }
}