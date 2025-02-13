namespace Ecommerce_site.Dto.Request.CategoryRequest;

public class CategoryStatusChangeRequest
{
    public required long CategoryId { get; set; }
    public required long AdminId { get; set; }
}