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
            if (tags.Count != request.TagIds.Count)
                return new ApiStandardResponse<ProductCreateResponse>(StatusCodes.Status404NotFound,
                    "One or more tag not found");
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

    public async Task<ApiStandardResponse<ProductUpdateResponse?>> UpdateProductAsync(long id,
        ProductUpdateRequest request)
    {
        var product = await _productRepo.GetByIdAsync(id);

        if (product is null)
            return new ApiStandardResponse<ProductUpdateResponse?>(StatusCodes.Status404NotFound,
                "The product does not exist");

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

    public async Task<ApiStandardResponse<ProductByIdResponse>> GetProductByIdAsync(long id)
    {
        var product = await _productRepo.GetSelectedColumnsByConditionAsync(
            p => p.ProductId == id && !p.IsDeleted,
            p => new ProductByIdResponse
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                Description = p.Description,
                CategoryName = p.Category.CategoryName,
                Quantity = p.Quantity,
                Discount = p.DiscountPercentage,
                Tags = p.Tags.Select(t => t.TagName).ToImmutableList(),
                ImageUrls = p.ProductImages.Select(i => i.ImageUrl).ToImmutableList(),
            }
        );

        if (product is null)
            return new ApiStandardResponse<ProductByIdResponse>(StatusCodes.Status404NotFound,
                "the product does not exist");

        return new ApiStandardResponse<ProductByIdResponse>(StatusCodes.Status200OK, product);
    }

    public async Task<ApiStandardResponse<PaginatedProductResponse>> GetAllProductAsync(long cursorValue = 0,
        int pageSize = 10)
    {
        if (pageSize < 1) pageSize = 10;


        var products = await _productRepo.GetCursorPaginatedSelectedColumnsAsync(
            p => true,
            p => new PaginatedProduct
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Discount = p.DiscountPercentage,
                Quantity = p.Quantity,
                Price = p.Price,
                Tags = p.Tags.Select(t => t.TagName).ToImmutableList(),
                ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary).Select(pi => pi.ImageUrl).First(),
                CategoryName = p.Category.CategoryName
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


    public async Task<ApiStandardResponse<ConfirmationResponse?>> ChangeProductImageAsync(
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

    public async Task<ApiStandardResponse<ConfirmationResponse>> ProductTagRemoveAsync(long id,
        ProductTagRemoveRequest request)
    {
        var product = await _productRepo.GetByConditionAsync(
            p => p.ProductId == id && !p.IsDeleted,
            p => p.Include(pt => pt.Tags) // Load tags
        );

        if (product is null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The product does not exist ");

        var tagsToRemove = product.Tags.Where(t => request.TagIds.Contains(t.TagId)).ToList();
        if (!tagsToRemove.Any())
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The tag does not exist");
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


    public async Task<ApiStandardResponse<ProductStatusResponse?>> ChangeProductStatusAsync(long id)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(id);

            if (product is null)
                return new ApiStandardResponse<ProductStatusResponse?>(StatusCodes.Status404NotFound,
                    $"The product does not exist");

            product.IsAvailable = !product.IsAvailable;
            return new ApiStandardResponse<ProductStatusResponse?>(StatusCodes.Status200OK, new ProductStatusResponse
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

    public async Task<ApiStandardResponse<ConfirmationResponse>> AddTagsToProduct(long productId,
        AddTagToProductRequest request)
    {
        if (!request.TagIds.Any())
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest,
                "No tag IDs provided");

        var product = await _productRepo.GetByConditionAsync(
            p => p.ProductId == productId && !p.IsDeleted,
            p => p.Include(pt => pt.Tags)
        );

        if (product is null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                $"The product with ID {productId} does not exist");

        var tagsToAdd = await _tagRepo.GetAllByConditionAsync(
            t => request.TagIds.Contains(t.TagId) && !t.IsDeleted
        );

        if (tagsToAdd.Count != request.TagIds.Count)
        {
            var missingTagIds = request.TagIds.Except(tagsToAdd.Select(t => t.TagId)).ToList();
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                $"The following tag IDs were not found: {string.Join(", ", missingTagIds)}");
        }

        var existingTagIds = product.Tags.Select(t => t.TagId).ToList();
        var newTags = tagsToAdd.Where(t => !existingTagIds.Contains(t.TagId)).ToList();

        if (newTags.Count == 0)
        {
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK,
                new ConfirmationResponse
                {
                    Message = "All specified tags are already associated with the product"
                });
        }

        foreach (var tag in newTags)
        {
            product.Tags.Add(tag);
        }

        await _productRepo.UpdateAsync(product);

        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK,
            new ConfirmationResponse
            {
                Message =
                    $"Successfully added {newTags.Count} tag(s) to the product: {string.Join(", ", newTags.Select(t => t.TagName))}"
            });
    }


    Task<ApiStandardResponse<PaginatedProductResponse>> IProductService.SearchProductAsync(string name)
    {
        throw new NotImplementedException();
    }
}