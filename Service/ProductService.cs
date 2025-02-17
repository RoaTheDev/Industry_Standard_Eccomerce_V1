using Ecommerce_site.Data;
using Ecommerce_site.Dto;
using Ecommerce_site.Dto.Request.ProductRequest;
using Ecommerce_site.Dto.response.ProductResponse;
using Ecommerce_site.Exception;
using Ecommerce_site.Model;
using Ecommerce_site.Repo.IRepo;
using Ecommerce_site.Service.IService;
using ILogger = Serilog.ILogger;

namespace Ecommerce_site.Service;

public class ProductService : IProductService
{
    private readonly IGenericRepo<Product> _productRepo;
    private readonly IGenericRepo<ProductImage> _imageRepo;
    private readonly ILogger _logger;
    private readonly EcommerceSiteContext _ecommerceSiteContext;
    private readonly IGenericRepo<Tag> _tagRepo;

    public ProductService(IGenericRepo<Product> productRepo, ILogger logger, IGenericRepo<ProductImage> imageRepo,
        EcommerceSiteContext ecommerceSiteContext, IGenericRepo<Tag> tagRepo)
    {
        _productRepo = productRepo;
        _logger = logger;
        _imageRepo = imageRepo;
        _ecommerceSiteContext = ecommerceSiteContext;
        _tagRepo = tagRepo;
    }


    public async Task<ApiStandardResponse<ProductCreateResponse>> CreateProductAsync(ProductCreateRequest request)
    {
        await using var transaction = await _ecommerceSiteContext.Database.BeginTransactionAsync();

        try
        {
            Product product = await _productRepo.AddAsync(new Product
            {
                ProductName = request.ProductName,
                Price = request.Price,
                Description = request.Description,
                CreatedBy = request.CreateBy,
                Quantity = request.Quantity,
                IsAvailable = request.IsAvailable!.Value,
                DiscountPercentage = request.Discount,
                CategoryId = request.CategoryId,
            });

            var tags = await _tagRepo.GetAllByConditionAsync(t => request.TagIds.Contains(t.TagId));
            product.Tags = tags;

            _logger.Information("Product-Tag insert success");

            await _productRepo.AddAsync(product);
            _logger.Information("Product insert success");


            List<ProductImage> images = new List<ProductImage>();
            int firstImgCount = 1;
            bool isPrimeImg = true;

            foreach (string image in request.ImageUrls)
            {
                images.Add(new ProductImage
                {
                    ImageUrl = image,
                    ProductId = product.ProductId,
                    IsPrimary = firstImgCount == 1 ? isPrimeImg : !isPrimeImg
                });
                firstImgCount++;
            }
            
            await _imageRepo.AddBulkAsync(images);
            _logger.Information("Image bulk insert success");

            await transaction.CommitAsync();

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
            await transaction.RollbackAsync();
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

    public async Task<ApiStandardResponse<ProductResponse>> GetProductByIdAsync(long id)
    {
        // var Product = await _productRepo.GetSelectedColumnsByConditionAsync(); 
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<IEnumerable<ProductResponse>>> GetProductsLikeNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<ApiStandardResponse<ProductImageChangeResponse>> ChangeProductImageAsync(
        ProductImageChangeRequest request)
    {
        throw new NotImplementedException();
    }

    public Task ProductTagRemoveAsync(ProductTagRemoveRequest request)
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