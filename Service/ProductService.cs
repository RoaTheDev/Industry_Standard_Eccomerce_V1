using System.Collections.Immutable;
using System.Linq.Expressions;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Exception;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Util.storage;
using LinqKit;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class ProductService : IProductService
{
    private readonly IGenericRepo<Product> _productRepo;
    private readonly IGenericRepo<Category> _categoryRepo;
    private readonly IGenericRepo<ProductImage> _imageRepo;
    private readonly ILogger _logger;
    private readonly IGenericRepo<Tag> _tagRepo;
    private readonly IStorageProvider _storageProvider;

    public ProductService(IGenericRepo<Product> productRepo, ILogger logger, IGenericRepo<ProductImage> imageRepo,
        IGenericRepo<Tag> tagRepo, [FromKeyedServices("local")] IStorageProvider storageProvider,
        IGenericRepo<Category> categoryRepo)
    {
        _productRepo = productRepo;
        _logger = logger;
        _imageRepo = imageRepo;
        _tagRepo = tagRepo;
        _storageProvider = storageProvider;
        _categoryRepo = categoryRepo;
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
        if (cursorValue < 0) cursorValue = 0;
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
                Tags = p.Tags.Select(t => t.TagName)
                    .ToImmutableList(),
                ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary)
                                .Select(pi => pi.ImageUrl)
                                .FirstOrDefault() ??
                            "",
                CategoryName = p.Category.CategoryName,
                CreateAt = p.CreatedAt
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

    public async Task<ApiStandardResponse<ConfirmationResponse>> ProductTagRemoveAsync(long id,
        ProductTagRemoveRequest request)
    {
        var product = await _productRepo.GetByConditionAsync(
            p => p.ProductId == id && !p.IsDeleted,
            p => p.Include(pt => pt.Tags)
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


    public async Task<ApiStandardResponse<ProductStatusResponse?>> UpdateProductStatusAsync(long id)
    {
        try
        {
            var product = await _productRepo.GetByIdAsync(id);

            if (product is null)
                return new ApiStandardResponse<ProductStatusResponse?>(StatusCodes.Status404NotFound,
                    $"The product does not exist");
            if (product.Quantity == 0)
                product.IsAvailable = false;

            product.IsAvailable = !product.IsAvailable;

            await _productRepo.UpdateAsync(product);

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

    public async Task<ApiStandardResponse<ConfirmationResponse>> UpdateProductCategory(long productId, long categoryId)
    {
        if (!await _categoryRepo.EntityExistByConditionAsync(c => c.CategoryId == categoryId))
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The category does not exist ");

        var product = await _productRepo.GetByIdAsync(productId);

        if (product is null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The product does not exist");

        product.CategoryId = categoryId;
        product.UpdatedAt = DateTime.UtcNow;

        await _productRepo.UpdateAsync(product);

        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = "Product category updated successfully"
        });
    }

    public async Task<ApiStandardResponse<ConfirmationResponse>> UpdateProductStockAsync(long productId,
        int stockQuantity)
    {
        if (stockQuantity < 0)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status400BadRequest,
                "Stock quantity cannot be negative");

        var product = await _productRepo.GetByIdAsync(productId);

        if (product is null)
            return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status404NotFound,
                "The product does not exist");

        product.Quantity = stockQuantity;
        product.UpdatedAt = DateTime.UtcNow;

        if (stockQuantity == 0)
            product.IsAvailable = false;

        await _productRepo.UpdateAsync(product);

        return new ApiStandardResponse<ConfirmationResponse>(StatusCodes.Status200OK, new ConfirmationResponse
        {
            Message = $"Product stock updated to {stockQuantity} units"
        });
    }

    public async Task<ApiStandardResponse<PaginatedProductResponse>> GetProductsByCategoryAsync(long categoryId,
        long cursorValue = 0, int pageSize = 10)
    {
        if (cursorValue < 0) cursorValue = 0;
        if (pageSize < 1) pageSize = 10;

        var products = await _productRepo.GetCursorPaginatedSelectedColumnsAsync(
            p => p.CategoryId == categoryId && !p.IsDeleted,
            p => new PaginatedProduct
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Discount = p.DiscountPercentage,
                Quantity = p.Quantity,
                Price = p.Price,
                Tags = p.Tags.Select(t => t.TagName)
                    .ToImmutableList(),
                ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary)
                                .Select(pi => pi.ImageUrl)
                                .FirstOrDefault() ??
                            "",
                CategoryName = p.Category.CategoryName,
                CreateAt = p.CreatedAt
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
            PageSize = pageSize,
            AppliedFilters = new AppliedProductFilters { CategoryId = categoryId }
        };

        return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK, response);
    }


    public async Task<ApiStandardResponse<PaginatedProductResponse>> GetNewArrivalsAsync(long cursorValue = 0,
        int pageSize = 10)
    {
        if (pageSize < 1) pageSize = 10;

        var products = await _productRepo.GetCursorPaginatedSelectedColumnsAsync(
            p => !p.IsDeleted,
            p => new PaginatedProduct
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Description = p.Description,
                Discount = p.DiscountPercentage,
                Quantity = p.Quantity,
                Price = p.Price,
                Tags = p.Tags.Select(t => t.TagName)
                    .ToImmutableList(),
                ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary)
                                .Select(pi => pi.ImageUrl)
                                .FirstOrDefault() ??
                            "",
                CategoryName = p.Category.CategoryName,
                CreateAt = p.CreatedAt
            },
            p => p.CreatedAt,
            cursorValue > 0 ? new DateTime(cursorValue) : DateTime.MinValue,
            pageSize,
            false
        );

        long? nextCursor = null;
        if (products.Count == pageSize)
        {
            var lastProduct = products.Last();
            var lastDateTime = await _productRepo.GetSelectedColumnsByConditionAsync(
                p => p.ProductId == lastProduct.ProductId,
                p => p.CreatedAt
            );

            nextCursor = lastDateTime.Ticks;
        }

        var response = new PaginatedProductResponse
        {
            Products = products,
            NextCursor = nextCursor,
            PageSize = pageSize,
            AppliedFilters = new AppliedProductFilters { SortBy = "date", SortOrder = "desc" }
        };

        return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK, response);
    }

    public Task<ApiStandardResponse<PaginatedProductResponse>> GetBestSellingProductsAsync(long cursorValue = 0,
        int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<PaginatedProductResponse>> SearchProductsAsync(string searchQuery,
        long cursorValue = 0, int pageSize = 10)
    {
        throw new NotImplementedException();
    }

    public async Task<ApiStandardResponse<PaginatedProductResponse>> GetFilteredProductsAsync(
        ProductFilterRequest filter,
        long cursorValue = 0,
        int pageSize = 10)
    {
        pageSize = Math.Max(1, pageSize);

        var productSortMappings = new Dictionary<string, (Expression<Func<Product, object>> Selector, bool Ascending)>
        {
            ["price_asc"] = (p => p.Price, true),
            ["price_desc"] = (p => p.Price, false),
            ["name"] = (p => p.ProductName, true),
            ["date"] = (p => p.CreatedAt, false),
        };

        var predicate = BuildFilterPredicate(filter);

        var (sortSelector, ascending) = GetSortSelector(filter.SortBy ?? string.Empty, productSortMappings);

        var products =
            await GetPaginatedProductsAsync(filter, cursorValue, pageSize, predicate, sortSelector, ascending);

        var nextCursor = GetNextCursor(products, filter.IsLatest, pageSize);

        var appliedFilters = GetAppliedFilters(filter, ascending);

        var response = new PaginatedProductResponse
        {
            Products = products.ToList(),
            NextCursor = nextCursor,
            PageSize = pageSize,
            AppliedFilters = appliedFilters
        };

        return new ApiStandardResponse<PaginatedProductResponse>(StatusCodes.Status200OK, response);
    }

    private Expression<Func<Product, bool>> BuildFilterPredicate(ProductFilterRequest filter)
    {
        var predicate = PredicateBuilder.New<Product>(p => !p.IsDeleted);

        if (filter.CategoryId.HasValue)
            predicate = predicate.And(p => p.CategoryId == filter.CategoryId.Value);

        if (filter.MinPrice.HasValue)
            predicate = predicate.And(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            predicate = predicate.And(p => p.Price <= filter.MaxPrice.Value);

        if (filter.InStockOnly.HasValue)
            predicate = predicate.And(p => p.IsAvailable == filter.InStockOnly.Value);

        if (filter.TagIds?.Count > 0)
            predicate = predicate.And(p => filter.TagIds.All(tagId => p.Tags.Any(t => t.TagId == tagId)));

        return predicate;
    }

    private (Expression<Func<Product, object>> sortSelector, bool ascending) GetSortSelector(
        string sortBy,
        Dictionary<string, (Expression<Func<Product, object>> Selector, bool Ascending)> productSortMappings)
    {
        return productSortMappings.TryGetValue(sortBy.ToLower(), out var mapping)
            ? mapping
            : (p => p.ProductId, true);
    }

    private async Task<IReadOnlyList<PaginatedProduct>> GetPaginatedProductsAsync(
        ProductFilterRequest filter,
        long cursorValue,
        int pageSize,
        Expression<Func<Product, bool>> predicate,
        Expression<Func<Product, object>> sortSelector,
        bool ascending)
    {
        return filter.IsLatest
            ? await _productRepo.GetCursorPaginatedSelectedColumnsAsync(
                predicate,
                p => new PaginatedProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Discount = p.DiscountPercentage,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Tags = p.Tags.Select(t => t.TagName),
                    ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefault() ?? string.Empty,
                    CategoryName = p.Category.CategoryName,
                    CreateAt = p.CreatedAt
                },
                p => p.CreatedAt,
                cursorValue > 0 ? new DateTime(cursorValue) : DateTime.MinValue,
                query => query.Include(p => p.Tags)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages),
                pageSize,
                false
            )
            : await _productRepo.GetCursorPaginatedSelectedColumnsAsync(
                predicate,
                p => new PaginatedProduct
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Description = p.Description,
                    Discount = p.DiscountPercentage,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Tags = p.Tags.Select(t => t.TagName),
                    ImageUrls = p.ProductImages.Where(pi => pi.IsPrimary)
                        .Select(pi => pi.ImageUrl)
                        .FirstOrDefault() ?? string.Empty,
                    CategoryName = p.Category.CategoryName,
                    CreateAt = p.CreatedAt
                },
                sortSelector,
                cursorValue,
                query => query.Include(p => p.Tags)
                    .Include(p => p.Category)
                    .Include(p => p.ProductImages),
                pageSize,
                ascending
            );
    }

    private long? GetNextCursor(IReadOnlyList<PaginatedProduct> products, bool isLatest, int pageSize)
    {
        return products.Count == pageSize
            ? isLatest
                ? products.Last().CreateAt.Ticks
                : products.Last().ProductId
            : null;
    }

    private AppliedProductFilters GetAppliedFilters(ProductFilterRequest filter, bool ascending)
    {
        return new AppliedProductFilters
        {
            CategoryId = filter.CategoryId,
            TagIds = filter.TagIds,
            MinPrice = filter.MinPrice,
            MaxPrice = filter.MaxPrice,
            InStockOnly = filter.InStockOnly,
            SortBy = filter.SortBy,
            SortOrder = ascending ? "asc" : "desc"
        };
    }
}