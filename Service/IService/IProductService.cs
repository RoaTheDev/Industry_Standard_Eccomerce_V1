using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;

namespace Ecommerce_site.Service.IService;

public interface IProductService
{
    Task<ApiStandardResponse<ProductCreateResponse>> CreateProductAsync(ProductCreateRequest request);
    Task<ApiStandardResponse<ProductUpdateResponse?>> UpdateProductAsync(ProductUpdateRequest request);
    Task<ApiStandardResponse<ProductByIdResponse>> GetProductByIdAsync(long id);

    Task<ApiStandardResponse<PaginatedProductResponse>> GetAllProductAsync(long cursorValue = 0,
        int pageSize = 10);

    Task<ApiStandardResponse<ConfirmationResponse?>> DeleteProductImage(long productId, long imageId);
    Task<ApiStandardResponse<ProductStatusResponse>> ChangeProductStatusAsync(long id);
    Task<ApiStandardResponse<ProductImageResponse?>> AddProductImageAsync(long id, IList<IFormFile> files);

    Task<ApiStandardResponse<ConfirmationResponse?>> ChangeProductImageAsync(long productId, long imageId);

    Task<ApiStandardResponse<ConfirmationResponse>> ProductTagRemoveAsync(ProductTagRemoveRequest request);
    Task<ApiStandardResponse<PaginatedProductResponse>> SearchProductAsync(string name);
}