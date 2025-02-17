using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class ProductService : IProductService
{
    private readonly IGenericRepo<Product> _productRepo;
    private readonly ILogger _logger;
    public ProductService(IGenericRepo<Product> productRepo, ILogger logger)
    {
        _productRepo = productRepo;
        _logger = logger;
    }

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

    public Task<ApiStandardResponse<ProductImageChangeResponse>> ChangeProductImageAsync(ProductImageChangeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ProductStatusResponse>> ChangeProductStatusAsync(ProductStatusChangeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ProductImageAddResponse>> AddProductImageAsync(ProductImageAddRequest request)
    {
        throw new NotImplementedException();
    }
}