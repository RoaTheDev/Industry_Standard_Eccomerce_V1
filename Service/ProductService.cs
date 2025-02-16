using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Service.IService;

namespace Ecommerce_site.Service;

public class ProductService : IProductService
{
    public Task<ApiStandardResponse<ProductCreateResponse>> CreateProductAsync(ProductCreateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ProductUpdateResponse>> UpdateProductAsync(ProductUpdateRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ProductResponse>> GetProductByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<IEnumerable<ProductResponse>>> GetProductsLikeNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ProductImageChangeResponse>> ChangeProductImageAsync(ProductImageChanger request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ProductStatusResponse>> ChangeProductStatusAsync(ProductStatusChangerRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ProductImageAddResponse>> AddProductImageAsync(ProductImageAddRequest request)
    {
        throw new NotImplementedException();
    }
}