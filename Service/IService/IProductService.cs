using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;

namespace Ecommerce_site.Service.IService;

public interface IProductService
{
    Task<ApiStandardResponse<ProductCreateResponse>> CreateProductAsync(ProductCreateRequest request);
    Task<ApiStandardResponse<ProductUpdateResponse?>> UpdateProductAsync(ProductUpdateRequest request);
    Task<ApiStandardResponse<ProductResponse>> GetProductByIdAsync(long id);
    Task<ApiStandardResponse<IEnumerable<ProductResponse>>> GetProductsLikeNameAsync(string name);
    Task<ApiStandardResponse<ProductImageChangeResponse>> ChangeProductImageAsync(ProductImageChangeRequest request);
    Task<ApiStandardResponse<ProductStatusResponse>> ChangeProductStatusAsync(ProductStatusChangeRequest request);
    Task<ApiStandardResponse<ProductImageAddResponse>> AddProductImageAsync(ProductImageAddRequest request);
    Task ProductTagRemoveAsync(ProductTagRemoveRequest request);
}