using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.TagResponse;

namespace Ecommerce_site.Service.IService.IProduct;

public interface IProductTagService
{
    Task<ApiStandardResponse<ConfirmationResponse>> AddTagsToProduct(long productId, AddTagToProductRequest request);
    Task<ApiStandardResponse<ConfirmationResponse>> ProductTagRemoveAsync(long id, ProductTagRemoveRequest request);
}