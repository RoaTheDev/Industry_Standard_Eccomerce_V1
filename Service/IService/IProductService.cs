using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;

namespace Ecommerce_site.Service.IService;

public interface IProductService
{
    Task<ApiStandardResponse<ProductCreateResponse>> CreateProductAsync(ProductCreateRequest request);
    Task<ApiStandardResponse<ProductUpdateResponse?>> UpdateProductAsync(long id, ProductUpdateRequest request);
    Task<ApiStandardResponse<ProductByIdResponse>> GetProductByIdAsync(long id);

    Task<ApiStandardResponse<PaginatedProductResponse>> GetAllProductAsync(long cursorValue = 0,
        int pageSize = 10);

    Task<ApiStandardResponse<ProductStatusResponse?>> UpdateProductStatusAsync(long id);
    Task<ApiStandardResponse<ProductImageResponse?>> AddProductImageAsync(long id, IList<IFormFile> files);

    Task<ApiStandardResponse<ConfirmationResponse?>> UpdateProductImageAsync(long productId, long imageId,
        IFormFile file);

    Task<ApiStandardResponse<ConfirmationResponse?>> DeleteProductImage(long productId, long imageId);

    Task<ApiStandardResponse<ConfirmationResponse>> AddTagsToProduct(long productId, AddTagToProductRequest request);
    Task<ApiStandardResponse<ConfirmationResponse>> ProductTagRemoveAsync(long id, ProductTagRemoveRequest request);
    Task<ApiStandardResponse<ConfirmationResponse>> SetPrimaryImageAsync(long productId, IFormFile file);

    Task<ApiStandardResponse<ConfirmationResponse>> UpdatePrimaryImageAsync(long productId, long imageId);
    Task<ApiStandardResponse<PaginatedProductResponse>> SearchProductsAsync(string searchQuery, long cursorValue = 0,
        int pageSize = 10);

    Task<ApiStandardResponse<ConfirmationResponse>> UpdateProductCategory(long productId, long categoryId);
    Task<ApiStandardResponse<ConfirmationResponse>> UpdateProductStockAsync(long productId, int stockQuantity);

    Task<ApiStandardResponse<PaginatedProductResponse>> GetProductsByCategoryAsync(
        long categoryId,
        long cursorValue = 0,
        int pageSize = 10
    );

    Task<ApiStandardResponse<PaginatedProductResponse>> GetNewArrivalsAsync(
        long cursorValue = 0,
        int pageSize = 10
    );

    Task<ApiStandardResponse<PaginatedProductResponse>> GetBestSellingProductsAsync(
        long cursorValue = 0,
        int pageSize = 10
    );

    Task<ApiStandardResponse<PaginatedProductResponse>> GetFilteredProductsAsync(
        ProductFilterRequest filter,
        long cursorValue = 0,
        int pageSize = 10
    );

  
}