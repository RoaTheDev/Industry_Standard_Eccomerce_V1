using System.Collections.Immutable;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Exception;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using Ecommerce_site.Service.IService.IProduct;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class ProductService : IProductService
{
    private readonly IGenericRepo<Product> _productRepo;
    private readonly IGenericRepo<Category> _categoryRepo;
    private readonly ILogger _logger;
    private readonly IGenericRepo<Tag> _tagRepo;

    public ProductService(IGenericRepo<Product> productRepo, ILogger logger,
        IGenericRepo<Tag> tagRepo, IGenericRepo<Category> categoryRepo)
    {
        _productRepo = productRepo;
        _logger = logger;
        _tagRepo = tagRepo;
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
        if (product.CategoryId != request.CategoryId)
            product.CategoryId = request.CategoryId;

        product.UpdatedBy = request.UpdatedBy;
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
}