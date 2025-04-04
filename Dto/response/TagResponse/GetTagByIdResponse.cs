namespace Ecommerce_site.Dto.response.TagResponse;

public class GetTagByIdResponse
{
    public required long TagId { get; set; }
    public required string TagName { get; set; }
    public required bool IsDeleted { get; set; }
}