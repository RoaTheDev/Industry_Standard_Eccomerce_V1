using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.CategoryRequest;
using Ecommerce_site.Dto.response.CategoryResponse;

namespace Ecommerce_site.Service.IService;

public interface ICategoryService
{
    Task<ApiStandardResponse<CategoryResponse?>> GetCategoryByIdAsync(long id);
    Task<ApiStandardResponse<CategoryResponse?>> GetCategoryLikeNameAsync(string name);
    Task<ApiStandardResponse<List<CategoryListResponse>?>> GetCategoryListByIdAsync(long id);
    Task<ApiStandardResponse<CategoryCreateResponse?>> CreateCategoryAsync(CategoryCreateRequest request);
    Task<ApiStandardResponse<CategoryResponse?>> UpdateCategoryAsync(CategoryUpdateRequest request);
    Task<ApiStandardResponse<ConfirmationResponse?>> CategoryStatusChangerAsync(CategoryStatusChangeRequest request);
}