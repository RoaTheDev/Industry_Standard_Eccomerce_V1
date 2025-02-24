using System.Collections.Immutable;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Exception;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util.storage;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class ProductService : IProductService
{
    private readonly IGenericRepo<Product> _productRepo;
    private readonly IGenericRepo<ProductImage> _imageRepo;
    private readonly ILogger _logger;
    private readonly IGenericRepo<Tag> _tagRepo;
    private readonly IStorageProvider _storageProvider;

    public ProductService(IGenericRepo<Product> productRepo, ILogger logger, IGenericRepo<ProductImage> imageRepo,
        IGenericRepo<Tag> tagRepo, [FromKeyedServices("local")] IStorageProvider storageProvider)
    {
        _productRepo = productRepo;
        _logger = logger;
        _imageRepo = imageRepo;
        _tagRepo = tagRepo;
        _storageProvider = storageProvider;
    }


    public async Task<ApiStandardResponse<ProductCreateResponse>> CreateProductAsync(ProductCreateRequest request)
    {
        try
        {
            Product product = new Product
            {
                ProductName = request.ProductName,
                Price = request.Price,
                Description = request.Description,
                CreatedBy = request.CreateBy,
                UpdatedBy = request.CreateBy,
                Quantity = request.Quantity,
                IsAvailable = request.IsAvailable!.Value,
                DiscountPercentage = request.Discount,
                CategoryId = request.CategoryId,
            };

            var tags = await _tagRepo.GetAllByConditionAsync(t => request.TagIds.Contains(t.TagId));
            product.Tags = tags;

            await _productRepo.AddAsync(product);

            return new ApiStandardResponse<ProductCreateResponse>(StatusCodes.Status201Created,
                new ProductCreateResponse
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Description = product.Description,
                    Discount = product.DiscountPercentage,
                    Price = product.Price,
                    Quantity = product.Quantity,
                    CreateAt = product.CreatedAt,
                    CreateBy = product.CreatedBy,
                    IsAvailable = product.IsAvailable,
                });
        }
        catch (System.Exception ex)
        {
            _logger.Error($"Error inserting product: {ex.Message}");
            throw;
        }
    }

    public async Task<ApiStandardResponse<ProductUpdateResponse?>> UpdateProductAsync(ProductUpdateRequest request)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(request.ProductId);
            if (!string.IsNullOrWhiteSpace(request.ProductName) && request.ProductName != product.ProductName)
                product.ProductName = request.ProductName;
            if (!string.IsNullOrWhiteSpace(request.Description) && request.Description != product.Description)
                product.Description = request.Description;
            if (product.DiscountPercentage != request.Discount)
                product.DiscountPercentage = request.Discount;
            if (product.Quantity != request.Quantity)
                product.Quantity = request.Quantity;
            if (product.UpdatedBy != request.UpdatedBy)
                product.DiscountPercentage = request.Discount;
            if (product.CategoryId != request.CategoryId)
                product.CategoryId = request.CategoryId;

            product.UpdatedAt = DateTime.UtcNow;
            await _productRepo.UpdateAsync(product);

            return new ApiStandardResponse<ProductUpdateResponse?>(StatusCodes.Status200OK, new ProductUpdateResponse
            {
                Description = product.Description,
                Quantity = product.Quantity,
                Price = product.Price,
                Discount = product.DiscountPercentage,
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                UpdatedAt = product.UpdatedAt
            });
        }
        catch (EntityNotFoundException e)
        {
            _logger.Error($"Error updating since the product does not exist by {e.Message}");
            return new ApiStandardResponse<ProductUpdateResponse?>(StatusCodes.Status404NotFound,
                "The product does not exist", null);
        }
    }

    public async Task<ApiStandardResponse<ProductByIdResponse>> GetProductByIdAsync(long id)
    {
        var product = await _productRepo.GetSelectedColumnsByConditionAsync(
            p => p.ProductId == id && !p.IsDeleted,
            p => new
            {
                p.ProductId,
                p.ProductName,
                p.Price,
                p.Description,
                p.Category.CategoryName,
                p.Quantity,
                p.DiscountPercentage,
                p.IsAvailable,
                Tags = p.Tags.Select(t => t.TagName).ToImmutableList(),
                ImageUrls = p.ProductImages.Select(i => i.ImageUrl).ToImmutableList()
            },
            p => p.Include(pt => pt.Tags)
                .Include(pc => pc.Category)
                .Include(pi => pi.ProductImages)
        );

        return new ApiStandardResponse<ProductByIdResponse>(StatusCodes.Status200OK, new ProductByIdResponse
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Description = product.Description,
            Discount = product.DiscountPercentage,
            Price = product.Price,
            Quantity = product.Quantity,
            CategoryName = product.CategoryName,
            Tags = product.Tags,
            ImageUrls = product.ImageUrls
        });
    }

    public async Task<ApiStandardResponse<PaginatedProductResponse>> GetAllProductAsync(long cursorValue = 0,
        int pageSize = 10)
    {
        if (pageSize < 1) pageSize = 10;


        var products = await _productRepo.GetCursorPaginatedSelectedColumnsAsync(
            p => true,
            p => new ProductResponse
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Discount = p.DiscountPercentage,
                Quantity = p.Quantity,
                Price = p.Price
            },
            p => p.ProductId,
            cursorValue,
            pageSize
        );

        long? nextCursor = null;
        if (products.Count == pageSize)
        {
            nextCursor = products.Last().ProductId;
        }

        var response = new PaginatedProductResponse
        {
            Products = products,
            NextCursor = nextCursor,
            PageSize = pageSize
        };
        return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK, response);
    }


    public async Task<ApiStandardResponse<ConfirmationResponse?>> DeleteProductImage(long productId, long imageId)
    {
        try
        {
            var image = await _imageRepo.GetByConditionAsync(i => i.ImageId == imageId && i.ProductId == productId);

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
        catch (EntityNotFoundException)
        {
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                $"The product does not exist", null);
        }
    }


    public async Task<ApiStandardResponse<ConfirmationResponse?>> ChangeProductImageAsync(
        long productId, long imageId)
    {
        try
        {
            var productImage = await _imageRepo.GetByConditionAsync(pi =>
                pi.ProductId == productId && pi.ImageId == imageId);

            if (productImage == null)
                throw new EntityNotFoundException(typeof(ProductImage), $"{productId} and {imageId}");

            string oldImagePath = productImage.ImageUrl;

            if (!string.IsNullOrWhiteSpace(oldImagePath))
            {
                await _storageProvider.DeleteFileAsync(oldImagePath);
            }

            await _imageRepo.DeleteAsync(productImage);

            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status200OK,
                new ConfirmationResponse()
                {
                    Message = "The image has been removed successfully"
                });
        }
        catch (EntityNotFoundException)
        {
            return new ApiStandardResponse<ConfirmationResponse?>(StatusCodes.Status404NotFound,
                $"The product does not exist", null);
        }
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> ProductTagRemoveAsync(ProductTagRemoveRequest request)
    {
        var product = await _productRepo.GetByConditionAsync(
            p => p.ProductId == request.ProductId && !p.IsDeleted,
            p => p.Include(pt => pt.Tags) // Load tags
        );

        var tagsToRemove = product.Tags.Where(t => request.TagIds.Contains(t.TagId)).ToList();
        if (!tagsToRemove.Any())
            throw new EntityNotFoundException(typeof(Tag), string.Join(", ", request.TagIds));
        foreach (var tag in tagsToRemove)
        {
            product.Tags.Remove(tag);
        }

        await _productRepo.UpdateAsync(product);
        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = "Product tag remove successfully"
        });
    }


    public async Task<ApiStandardResponse<ProductStatusResponse>> ChangeProductStatusAsync(long id)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(id);

            product.IsAvailable = !product.IsAvailable;
            return new ApiStandardResponse<ProductStatusResponse>(StatusCodes.Status200OK, new ProductStatusResponse
            {
                ProductId = product.ProductId,
                IsAvailable = product.IsAvailable
            });
        }
        catch (EntityNotFoundException)
        {
            throw new EntityNotFoundException(typeof(Product), id);
        }
    }

    public async Task<ApiStandardResponse<ProductImageResponse?>> AddProductImageAsync(long id,
        IList<IFormFile> files)
    {
        try
        {
            var product = await _productRepo.GetByConditionAsync(p => p.ProductId == id);
            IList<ProductImage> productImages = new List<ProductImage>();
            bool isPrimary = true;
            int counter = 1;

            var imageUrls = await _storageProvider.UploadFilesAsync<ProductImage>(Guid.NewGuid(), files.ToList());
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
        catch (EntityNotFoundException)
        {
            return new ApiStandardResponse<ProductImageResponse?>(StatusCodes.Status404NotFound,
                $"The product does not exist", null);
        }
    }

    Task<ApiStandardResponse<PaginatedProductResponse>> IProductService.SearchProductAsync(string name)
    {
        throw new NotImplementedException();
    }
}