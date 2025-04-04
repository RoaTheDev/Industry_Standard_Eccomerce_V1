using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.TagRequest;
using Ecommerce_site.Dto.response.TagResponse;

namespace Ecommerce_site.Service.IService;

public interface ITagService
{
    Task<ApiStandardResponse<List<AllTagResponse>>> GetAllTagsAsync();
    Task<ApiStandardResponse<ConfirmationResponse>> CreateTagAsync(long adminId, CreateTagRequest request);
    Task<ApiStandardResponse<ConfirmationResponse>> UpdateTagAsync(long id, long adminId, UpdateTagRequest request);
    Task<ApiStandardResponse<ConfirmationResponse>> DeleteTagAsync(long id, long adminId);
    Task<ApiStandardResponse<GetTagByIdResponse>> GetTagByIdAsync(long id);
}