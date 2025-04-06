namespace Ecommerce_site.Dto.response.TagResponse;

public class PaginatedTagResponse
{
    public required List<AllTagResponse> Tags { get; set; }
    public long? NextCursor { get; set; }
    public required int PageSize { get; set; }
}