using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;

namespace Ecommerce_site.Service.IService;

public interface IProductService
{
    Task<ApiStandardResponse<ProductCreateResponse>> CreateProductAsync(ProductCreateRequest request);
    Task<ApiStandardResponse<ProductUpdateResponse?>> UpdateProductAsync(ProductUpdateRequest request);
    Task<ApiStandardResponse<ProductByIdResponse>> GetProductByIdAsync(long id);
    Task<ApiStandardResponse<ConfirmationResponse?>> DeleteProductImage(long productId, long imageId);

    Task<ApiStandardResponse<IEnumerable<ProductResponse>>> SearchProductAsync(string name);

    Task<ApiStandardResponse<ProductImageChangeResponse?>> ChangeProductImageAsync(long productId, long imageId,
        IFormFile file);

    Task<ApiStandardResponse<ProductStatusResponse>> ChangeProductStatusAsync(long id);
    Task<ApiStandardResponse<ProductImageResponse?>> AddProductImageAsync(long id, IList<IFormFile> files);
    Task<ApiStandardResponse<ConfirmationResponse>> ProductTagRemoveAsync(ProductTagRemoveRequest request);
}