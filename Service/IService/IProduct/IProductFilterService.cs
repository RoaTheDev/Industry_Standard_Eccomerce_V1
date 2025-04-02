using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;

namespace Ecommerce_site.Service.IService;

public interface IProductFilterService
{
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