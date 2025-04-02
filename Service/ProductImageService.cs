using Ecommerce_site.Dto;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Service.IService.IProduct;
using Ecommerce_site.Util.storage;

namespace Ecommerce_site.Service;

public class ProductImageService : IProductImageService
{
    private readonly IGenericRepo<Product> _productRepo;
    private readonly IGenericRepo<ProductImage> _imageRepo;
    private readonly IStorageProvider _storageProvider;
    public ProductImageService(IGenericRepo<Product> productRepo, IGenericRepo<ProductImage> imageRepo, [FromKeyedServices("local")] IStorageProvider storageProvider)
    {
        _productRepo = productRepo;
        _imageRepo = imageRepo;
        _storageProvider = storageProvider;
    }

    public async Task<ApiStandardResponse<ProductImageResponse?>> AddProductImageAsync(long id,
        IList<IFormFile> files)
    {
        var product = await _productRepo.GetByConditionAsync(p => p.ProductId == id);
        if (product is null)
            return new ApiStandardResponse<ProductImageResponse?>(StatusCodes.Status404NotFound,
                $"The product does not exist");
        IList<ProductImage> productImages = new List<ProductImage>();
        bool isPrimary = true;
        int counter = 1;

        var imageUrls = await _storageProvider.UploadFileAsync<ProductImage>(Guid.NewGuid(), files.ToList());
        foreach (var url in imageUrls)
        {
            productImages.Add(new ProductImage
            {
                ImageUrl = url,
                ProductId = product.ProductId,
                IsPrimary = counter == 1 ? isPrimary : !isPrimary
            });
            counter++;
        }

        IList<ProductImage> uploadedImages = await _imageRepo.AddBulkAsync(productImages);
        return new ApiStandardResponse<ProductImageResponse?>(StatusCodes.Status201Created,
            new ProductImageResponse
            {
                ProductId = product.ProductId,
                Images = uploadedImages.Select(img => new ImageResponse
                {
                    ImageId = img.ImageId,
                    ImageUrl = img.ImageUrl
                })
            });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse?>> DeleteProductImage(long productId, long imageId)
    {
        var image = await _imageRepo.GetByConditionAsync(i => i.ImageId == imageId && i.ProductId == productId);

        if (image is null)
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                $"The product does not exist");
        if (!string.IsNullOrWhiteSpace(image.ImageUrl))
        {
            await _storageProvider.DeleteFileAsync(image.ImageUrl);
        }

        await _imageRepo.DeleteAsync(image);
        return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = "The image has been delete successfully"
        });
    }


    public async Task<ApiStandardResponse<ConfirmationResponse?>> UpdateProductImageAsync(
        long productId, long imageId, IFormFile file)
    {
        var productImage = await _imageRepo.GetByConditionAsync(pi =>
            pi.ProductId == productId && pi.ImageId == imageId);

        if (productImage == null)
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                $"The product does not exist");

        string oldImagePath = productImage.ImageUrl;

        if (!string.IsNullOrWhiteSpace(oldImagePath))
        {
            await _storageProvider.DeleteFileAsync(oldImagePath);
        }

        string newImageUrl = await _storageProvider.UploadFileAsync<ProductImage>(Guid.NewGuid(), file);
        productImage.ImageUrl = newImageUrl;
        await _imageRepo.UpdateAsync(productImage);

        return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status200OK,
            new ConfirmationResponse()
            {
                Message = "The image has been changed successfully"
            });
    }

      public async Task<ApiStandardResponse<ConfirmationResponse>> SetPrimaryImageAsync(long productId, IFormFile file)
    {
        if (!await _productRepo.EntityExistByConditionAsync(p => p.ProductId == productId))
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The product does not exist");

        var currentPrimaryImage = await _imageRepo.GetByConditionAsync(i => i.ProductId == productId && i.IsPrimary);
        if (currentPrimaryImage is not null)
        {
            currentPrimaryImage.IsPrimary = false;
            await _imageRepo.UpdateAsync(currentPrimaryImage);
        }

        string imageUrl = await _storageProvider.UploadFileAsync<ProductImage>(Guid.NewGuid(), file);

        var newPrimaryImage = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsPrimary = true
        };

        await _imageRepo.AddAsync(newPrimaryImage);

        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = "Primary image set successfully"
        });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> UpdatePrimaryImageAsync(long productId, long imageId)
    {
        if (!await _productRepo.EntityExistByConditionAsync(p => p.ProductId == productId))
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The product does not exist");

        var image = await _imageRepo.GetByConditionAsync(i => i.ImageId == imageId && i.ProductId == productId);
        if (image == null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The image does not exist for this product");

        var currentPrimaryImage = await _imageRepo.GetByConditionAsync(i => i.ProductId == productId && i.IsPrimary);
        if (currentPrimaryImage != null)
        {
            if (currentPrimaryImage.ImageId == imageId)
                return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
                {
                    Message = "This image is already set as primary"
                });

            currentPrimaryImage.IsPrimary = false;
            await _imageRepo.UpdateAsync(currentPrimaryImage);
        }

        image.IsPrimary = true;
        await _imageRepo.UpdateAsync(image);

        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = "Primary image updated successfully"
        });
    }
}