using Ecommerce_site.Dto;
using Ecommerce_site.Dto.response.ProductResponse;

namespace Ecommerce_site.Service.IService.IProduct;

public interface IProductImageService
{
    Task<ApiStandardResponse<ProductImageResponse?>> AddProductImageAsync(long id, IList<IFormFile> files);

    Task<ApiStandardResponse<ConfirmationResponse?>> UpdateProductImageAsync(long productId, long imageId,
        IFormFile file);

    Task<ApiStandardResponse<ConfirmationResponse?>> DeleteProductImage(long productId, long imageId);
    Task<ApiStandardResponse<ConfirmationResponse>> SetPrimaryImageAsync(long productId, IFormFile file);

    Task<ApiStandardResponse<ConfirmationResponse>> UpdatePrimaryImageAsync(long productId, long imageId);

}