using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;

namespace Ecommerce_site.Service.IService.IProduct;

public interface IProductService
{
    Task<ApiStandardResponse<ProductCreateResponse>> CreateProductAsync(ProductCreateRequest request);
    Task<ApiStandardResponse<ProductUpdateResponse?>> UpdateProductAsync(long id, ProductUpdateRequest request);
    Task<ApiStandardResponse<ProductByIdResponse>> GetProductByIdAsync(long id);

    Task<ApiStandardResponse<PaginatedProductResponse>> GetAllProductAsync(long cursorValue = 0,
        int pageSize = 10);

    Task<ApiStandardResponse<ProductStatusResponse?>> UpdateProductStatusAsync(long id);
    Task<ApiStandardResponse<ConfirmationResponse>> UpdateProductCategory(long productId, long categoryId);
    Task<ApiStandardResponse<ConfirmationResponse>> UpdateProductStockAsync(long productId, int stockQuantity);

    Task<ApiStandardResponse<PaginatedProductResponse>> GetProductsByCategoryAsync(
        long categoryId,
        long cursorValue = 0,
        int pageSize = 10
    );


}